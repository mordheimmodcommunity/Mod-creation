using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverviewCamera : ICheapState
{
    private const float fixedCameraHeight = 40f;

    [HideInInspector]
    public Transform lookAtTarget;

    private Vector3 lookAtTargetTargetPosition;

    private static readonly int[] zoomLevels;

    public int zoomIdx;

    public float zoomSpeed;

    private float currentZoom;

    private float targetZoom;

    private float oldZoom;

    public float rotationAngle;

    public int rotationIdx;

    public float rotationSpeed;

    private float currentRotation;

    private float targetRotation;

    private float oldRotation;

    private int rotationDir;

    private float moveSpeed = 20f;

    public float moveSpeedMin = 15f;

    public float moveSpeedMax = 40f;

    public float moveSpeedIncrease = 5f;

    private float increaseTime;

    private float fromHeight;

    private float toHeight;

    private Bounds mapBounds;

    private int cyclingUnitIdx;

    private List<UnitController> cyclingUnits;

    private Vector3 camDir = new Vector3(0f, 1f, 0f);

    private Vector3 offsetVector = new Vector3(0f, 1.5f, 0f);

    private List<FlyingOverview> flyingOverviews = new List<FlyingOverview>();

    private int currentSelectedIdx = -1;

    private CameraManager mngr;

    private Camera cam;

    private Transform dummyCam;

    private List<MonoBehaviour> tempGetComponentsList = new List<MonoBehaviour>();

    private RaycastHit[] raycastHits = new RaycastHit[32];

    private bool active;

    public OverviewCamera(CameraManager camMngr)
    {
        mngr = camMngr;
        cam = mngr.gameObject.GetComponent<Camera>();
        dummyCam = camMngr.dummyCam.transform;
        PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<GameObject>("prefabs/camera/overview_target", delegate (Object o)
        {
            GameObject gameObject = (GameObject)Object.Instantiate(o);
            SceneManager.MoveGameObjectToScene(gameObject, camMngr.gameObject.scene);
            gameObject.transform.SetParent(null);
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            lookAtTarget = gameObject.transform;
            lookAtTarget.gameObject.SetActive(value: false);
        });
        zoomSpeed = 50f;
        zoomIdx = 2;
        currentZoom = zoomLevels[zoomIdx];
        rotationAngle = 90f;
        rotationSpeed = 180f;
        rotationIdx = 0;
        SetTargetRotation();
        cyclingUnitIdx = 0;
        cyclingUnits = new List<UnitController>();
    }

    static OverviewCamera()
    {
        zoomLevels = new int[4]
        {
            10,
            20,
            30,
            40
        };
    }

    public void Destroy()
    {
    }

    public void FixedUpdate()
    {
    }

    public void Enter(int from)
    {
        cyclingUnits.Clear();
        active = true;
        PandoraSingleton<GameManager>.Instance.StartCoroutine(DisplayFlyingOverviews());
        if (PandoraSingleton<GameManager>.Instance.TacticalViewHelpersEnabled)
        {
            PandoraSingleton<MissionManager>.Instance.ActivateMapObjectiveZones(activate: true);
        }
        lookAtTarget.gameObject.SetActive(value: true);
        moveSpeed = moveSpeedMin;
        if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().IsPlayed())
        {
            SetTarget(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().transform, immediate: true);
        }
        else if (PandoraSingleton<MissionManager>.Instance.StateMachine.GetActiveStateId() == 1)
        {
            List<SpawnNode> list = PandoraSingleton<MissionStartData>.Instance.spawnNodes[PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().idx];
            SetTarget(list[0].transform, immediate: true);
        }
        else
        {
            List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
            for (int i = 0; i < allUnits.Count; i++)
            {
                if (allUnits[i].IsPlayed() && allUnits[i].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    SetTarget(allUnits[i].transform, immediate: true);
                    break;
                }
            }
        }
        SetTargetZoom();
        EnterOrthoCam();
        PandoraSingleton<UIMissionManager>.Instance.ShowingOverview = true;
        PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(1);
        PandoraSingleton<UIMissionManager>.Instance.overview.OnEnable();
        PandoraSingleton<UIMissionManager>.Instance.HideUnitStats();
        PandoraSingleton<UIMissionManager>.Instance.HidePropsInfo();
        PandoraSingleton<UIMissionManager>.Instance.unitAction.OnDisable();
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.MENU);
    }

    public void Exit(int to)
    {
        active = false;
        PandoraSingleton<GameManager>.Instance.StopCoroutine(DisplayFlyingOverviews());
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.MENU);
        if (PandoraSingleton<MissionManager>.Exists())
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSLayer("characters");
            DeactivateFlyingOverviews();
            if (lookAtTarget != null && lookAtTarget.gameObject != null)
            {
                lookAtTarget.gameObject.SetActive(value: false);
            }
            ExitOrthoCam();
            if (PandoraSingleton<GameManager>.Instance.TacticalViewHelpersEnabled)
            {
                PandoraSingleton<MissionManager>.Instance.ActivateMapObjectiveZones(activate: false);
            }
            PandoraSingleton<UIMissionManager>.Instance.ShowingOverview = false;
            PandoraSingleton<UIMissionManager>.Instance.HidePropsInfo();
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, PandoraSingleton<MissionManager>.Instance.GetCurrentUnit());
            if (to == 7)
            {
                PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(6);
            }
            else
            {
                PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(0);
            }
            PandoraSingleton<UIMissionManager>.Instance.overview.OnDisable();
        }
    }

    private void DeactivateFlyingOverviews()
    {
        for (int i = 0; i < flyingOverviews.Count; i++)
        {
            if (flyingOverviews[i] != null)
            {
                flyingOverviews[i].Deactivate();
            }
        }
        flyingOverviews.Clear();
    }

    private IEnumerator DisplayFlyingOverviews()
    {
        DeactivateFlyingOverviews();
        yield return new WaitForSeconds(0.5f);
        PandoraSingleton<FlyingTextManager>.Instance.ResetWorldCorners();
        List<MapImprint> imprints = PandoraSingleton<MissionManager>.Instance.MapImprints;
        for (int i = 0; i < imprints.Count; i++)
        {
            if (i % 5 == 0)
            {
                yield return null;
            }
            flyingOverviews.Add(null);
            if (imprints[i].State == MapImprintStateId.VISIBLE || imprints[i].State == MapImprintStateId.LOST)
            {
                if (imprints[i].UnitCtrlr != null)
                {
                    cyclingUnits.Add(imprints[i].UnitCtrlr);
                }
                DisplayFlyingText(i);
            }
        }
        cyclingUnits.Sort(new LadderSorter());
    }

    private void DisplayFlyingText(int idx)
    {
        PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.OVERVIEW_UNIT, delegate (FlyingText flyingText)
        {
            FlyingOverview flyingOverview = (FlyingOverview)flyingText;
            if (!active || idx >= flyingOverviews.Count)
            {
                flyingOverview.Deactivate();
            }
            else
            {
                MapImprint mapImprint = PandoraSingleton<MissionManager>.Instance.MapImprints[idx];
                bool clamp = false;
                MapBeacon mapBeacon = null;
                List<MapBeacon> mapBeacons = PandoraSingleton<MissionManager>.Instance.GetMapBeacons();
                for (int i = 0; i < mapBeacons.Count; i++)
                {
                    if (mapBeacons[i].imprint == mapImprint)
                    {
                        mapBeacon = mapBeacons[i];
                        clamp = true;
                    }
                }
                List<WarbandController> warbandCtrlrs = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs;
                for (int j = 0; j < warbandCtrlrs.Count; j++)
                {
                    if (warbandCtrlrs[j].wagon != null && warbandCtrlrs[j].wagon.mapImprint == mapImprint)
                    {
                        clamp = true;
                    }
                }
                if (mapBeacon != null)
                {
                    mapBeacon.flyingOverview = flyingOverview;
                    mapBeacon.Refresh();
                    flyingOverview.gameObject.SetActive(mapBeacon.isActiveAndEnabled);
                }
                flyingOverview.Set(mapImprint, clamp, selected: false);
                if (flyingOverviews[idx] != null)
                {
                    flyingOverviews[idx].Deactivate();
                }
                flyingOverviews[idx] = flyingOverview;
            }
        });
    }

    public void Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("restore_map", 6))
        {
            if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().IsPlayed())
            {
                SetTarget(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().transform, immediate: true);
            }
            else
            {
                SetTarget(PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().GetLeader().transform, immediate: true);
            }
            zoomIdx = zoomLevels.Length - 1;
            SetTargetZoom();
            rotationIdx = 0;
            SetTargetRotation();
        }
        if (PandoraSingleton<MissionManager>.Instance.StateMachine.GetActiveStateId() != 1)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("cycling", 6))
            {
                cyclingUnitIdx = ((cyclingUnitIdx - 1 < 0) ? (cyclingUnits.Count - 1) : (cyclingUnitIdx - 1));
                SetTarget(cyclingUnits[cyclingUnitIdx].Imprint.lastKnownPos);
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cycling", 6))
            {
                cyclingUnitIdx = ((cyclingUnitIdx + 1 < cyclingUnits.Count) ? (cyclingUnitIdx + 1) : 0);
                SetTarget(cyclingUnits[cyclingUnitIdx].Imprint.lastKnownPos);
            }
        }
        Vector3 position = Vector3.Lerp(lookAtTarget.position, lookAtTargetTargetPosition, 0.25f);
        if (currentSelectedIdx != -1)
        {
            moveSpeed = moveSpeedMin;
            increaseTime = 0f;
        }
        float axisRaw = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("h", 6);
        float axisRaw2 = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("v", 6);
        axisRaw += PandoraSingleton<PandoraInput>.Instance.GetAxis("scroll_map_x", 6);
        axisRaw2 += PandoraSingleton<PandoraInput>.Instance.GetAxis("scroll_map_y", 6);
        if (axisRaw != 0f || axisRaw2 != 0f)
        {
            Vector3 forward = lookAtTarget.transform.forward;
            forward.y = 0f;
            Vector3 a = axisRaw * lookAtTarget.transform.right + axisRaw2 * forward;
            moveSpeed = ((moveSpeed + moveSpeedIncrease * increaseTime >= moveSpeedMax) ? moveSpeedMax : (moveSpeed + moveSpeedIncrease * increaseTime));
            increaseTime += Time.smoothDeltaTime;
            position += a * moveSpeed * Time.smoothDeltaTime;
            if (PandoraSingleton<MissionManager>.Instance.mapContour != null)
            {
                Vector2 vector = new Vector2(position.x, position.z);
                Vector2 pointInsideMeshEdges = PandoraUtils.GetPointInsideMeshEdges(PandoraSingleton<MissionManager>.Instance.mapContour.FlatEdges, PandoraSingleton<MissionManager>.Instance.mapContour.center, vector * -1000f, vector);
                position.x = pointInsideMeshEdges.x;
                position.z = pointInsideMeshEdges.y;
            }
            lookAtTargetTargetPosition = position;
        }
        else
        {
            moveSpeed = moveSpeedMin;
            increaseTime = 0f;
        }
        float num = zoomIdx + 1;
        num *= 2.3f;
        lookAtTarget.transform.localScale = new Vector3(num, num, num);
        lookAtTarget.transform.position = position;
        if (currentRotation != targetRotation)
        {
            currentRotation += rotationSpeed * Time.smoothDeltaTime * (float)rotationDir;
            if ((rotationDir == -1 && currentRotation < targetRotation) || (rotationDir == 1 && currentRotation > targetRotation))
            {
                currentRotation = targetRotation;
            }
        }
        lookAtTarget.transform.rotation = Quaternion.Euler(new Vector3(0f, currentRotation, 0f));
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("zoom", 6))
        {
            zoomIdx = ((zoomIdx - 1 < 0) ? (zoomLevels.Length - 1) : (zoomIdx - 1));
            SetTargetZoom();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("zoom_mouse", 6))
        {
            zoomIdx = ((zoomIdx - 1 < 0) ? zoomIdx : (zoomIdx - 1));
            SetTargetZoom();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("zoom_mouse", 6))
        {
            zoomIdx = ((zoomIdx + 1 >= zoomLevels.Length) ? zoomIdx : (zoomIdx + 1));
            SetTargetZoom();
        }
        if (currentZoom != targetZoom)
        {
            currentZoom += zoomSpeed * Time.smoothDeltaTime * (float)((!(currentZoom >= targetZoom)) ? 1 : (-1));
            if ((oldZoom >= targetZoom && currentZoom < targetZoom) || (oldZoom < targetZoom && currentZoom > targetZoom))
            {
                currentZoom = targetZoom;
            }
        }
        lookAtTarget.transform.position.Set(lookAtTarget.transform.position.x, 40f, lookAtTarget.transform.position.z);
        Camera main = Camera.main;
        main.orthographicSize = currentZoom;
        Vector3 position2 = lookAtTarget.transform.position;
        main.transform.position = position2;
        Quaternion rotation = Quaternion.LookRotation(Vector3.down);
        rotation *= Quaternion.Euler(0f, 0f, 0f - currentRotation);
        Vector3[] array = new Vector3[4]
        {
            main.ViewportToWorldPoint(new Vector3(0.5f, 1f, main.nearClipPlane)),
            main.ViewportToWorldPoint(new Vector3(1f, 0.5f, main.nearClipPlane)),
            main.ViewportToWorldPoint(new Vector3(0.5f, 0f, main.nearClipPlane)),
            main.ViewportToWorldPoint(new Vector3(0f, 0.5f, main.nearClipPlane))
        };
        Vector3 vector2 = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < array.Length; i++)
        {
            vector2 += array[i] - mapBounds.ClosestPoint(array[i]);
        }
        position2 -= vector2;
        if (array[1].x - array[3].x > mapBounds.size.x)
        {
            position2.x = mapBounds.center.x;
        }
        if (array[0].z - array[2].z > mapBounds.size.z)
        {
            position2.z = mapBounds.center.z;
        }
        position2.y = 40f;
        dummyCam.position = position2;
        dummyCam.rotation = rotation;
        Vector3 position3 = lookAtTarget.transform.position;
        position3.y = 0f;
        currentSelectedIdx = -1;
        float num2 = float.MaxValue;
        for (int j = 0; j < flyingOverviews.Count; j++)
        {
            if (flyingOverviews[j] != null && flyingOverviews[j].imprint != null)
            {
                Vector3 b = flyingOverviews[j].imprint.transform.position;
                MapImprint imprint = flyingOverviews[j].imprint;
                if (imprint.imprintType == MapImprintType.UNIT && imprint.State != 0)
                {
                    b = imprint.lastKnownPos;
                }
                b.y = 0f;
                float num3 = Vector3.SqrMagnitude(position3 - b);
                if (num3 < 4f && num3 < num2)
                {
                    num2 = num3;
                    currentSelectedIdx = j;
                }
            }
        }
        bool flag = true;
        bool flag2 = true;
        if (currentSelectedIdx != -1)
        {
            flag2 = false;
            MapImprint imprint2 = flyingOverviews[currentSelectedIdx].imprint;
            switch (imprint2.imprintType)
            {
                case MapImprintType.UNIT:
                    if (imprint2.State == MapImprintStateId.VISIBLE)
                    {
                        SelectCurrentUnit(imprint2.UnitCtrlr);
                        flag = false;
                    }
                    flag2 = true;
                    break;
                case MapImprintType.PLAYER_WAGON:
                case MapImprintType.ENEMY_WAGON:
                    SelectCurrentWagon(imprint2);
                    break;
                case MapImprintType.INTERACTIVE_POINT:
                case MapImprintType.WYRDSTONE:
                    SelectCurrentInteractive(imprint2.gameObject.GetComponent<InteractivePoint>());
                    break;
                case MapImprintType.TRAP:
                    SelectCurrentTrap(imprint2.Trap);
                    break;
                case MapImprintType.DESTRUCTIBLE:
                    SelectCurrentDestructible(imprint2.Destructible);
                    break;
                default:
                    currentSelectedIdx = -1;
                    flag = true;
                    flag2 = true;
                    break;
                case MapImprintType.BEACON:
                case MapImprintType.PLAYER_DEPLOYMENT:
                    break;
            }
        }
        SetSelectedOverview(currentSelectedIdx);
        if (flag)
        {
            PandoraSingleton<UIMissionManager>.Instance.HideUnitStats();
        }
        if (flag2)
        {
            PandoraSingleton<UIMissionManager>.Instance.HidePropsInfo();
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action", 6))
        {
            if (currentSelectedIdx != -1 && currentSelectedIdx < flyingOverviews.Count && flyingOverviews[currentSelectedIdx] != null && flyingOverviews[currentSelectedIdx].imprint != null && flyingOverviews[currentSelectedIdx].imprint.imprintType == MapImprintType.PLAYER_DEPLOYMENT)
            {
                MissionManager instance = PandoraSingleton<MissionManager>.Instance;
                SpawnNode component = flyingOverviews[currentSelectedIdx].imprint.gameObject.GetComponent<SpawnNode>();
                Deployment deployment = (Deployment)instance.StateMachine.GetState(instance.StateMachine.GetActiveStateId());
                int num4 = deployment.FindSpawnNodeIndex(component);
                object[] parms = new object[1]
                {
                    num4
                };
                deployment.RecenterCameraOnDeployedUnit();
                deployment.Send(reliable: true, Hermes.SendTarget.ALL, deployment.uid, 5u, parms);
                currentSelectedIdx = -1;
            }
            else if (currentSelectedIdx != -1 && currentSelectedIdx < flyingOverviews.Count && flyingOverviews[currentSelectedIdx] != null && flyingOverviews[currentSelectedIdx].imprint != null && flyingOverviews[currentSelectedIdx].imprint.Beacon != null && flyingOverviews[currentSelectedIdx].imprint.Beacon.isActiveAndEnabled)
            {
                PandoraSingleton<MissionManager>.Instance.RemoveMapBecon(flyingOverviews[currentSelectedIdx].imprint.Beacon);
                RefreshUI();
            }
            else
            {
                PandoraSingleton<MissionManager>.Instance.SpawnMapBeacon(lookAtTarget.transform.position, RefreshUI);
            }
        }
    }

    private void RefreshUI()
    {
        PandoraSingleton<UIMissionManager>.Instance.overview.Refresh(zoomIdx);
    }

    private void SelectCurrentInteractive(InteractivePoint currentInteractive)
    {
        PandoraSingleton<UIMissionManager>.Instance.SetPropsInfo(currentInteractive.Imprint.visibleTexture, PandoraSingleton<LocalizationManager>.Instance.GetStringById(currentInteractive.GetLocAction()));
    }

    private void SelectCurrentTrap(Trap currentTrap)
    {
        bool flag = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().teamIdx == currentTrap.TeamIdx;
        string key = "action_name_trap";
        if (!flag)
        {
            switch (currentTrap.enemyImprintType)
            {
                case MapImprintType.INTERACTIVE_POINT:
                    key = "action_name_scavenge";
                    break;
                case MapImprintType.WYRDSTONE:
                    key = "action_name_gather_wyrdstone";
                    break;
            }
        }
        PandoraSingleton<UIMissionManager>.Instance.SetPropsInfo(currentTrap.Imprint.visibleTexture, PandoraSingleton<LocalizationManager>.Instance.GetStringById(key));
    }

    private void SelectCurrentWagon(MapImprint imprint)
    {
        PandoraSingleton<UIMissionManager>.Instance.SetPropsInfo(imprint.visibleTexture, imprint.Wagon.chest.warbandController.name);
    }

    private void SelectCurrentUnit(UnitController currentUnit)
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_UNIT_CHANGED, currentUnit, v2: true);
        if (currentUnit.IsPlayed())
        {
            PandoraSingleton<UIMissionManager>.Instance.CurrentUnitController = currentUnit;
            if (currentUnit.Engaged)
            {
                PandoraSingleton<UIMissionManager>.Instance.CurrentUnitTargetController = currentUnit.EngagedUnits[0];
                PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(1);
            }
            else
            {
                PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(0);
            }
            return;
        }
        PandoraSingleton<UIMissionManager>.Instance.CurrentUnitTargetController = currentUnit;
        if (currentUnit.Engaged)
        {
            PandoraSingleton<UIMissionManager>.Instance.CurrentUnitController = currentUnit.EngagedUnits[0];
            PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(1);
        }
        else
        {
            PandoraSingleton<UIMissionManager>.Instance.unitCombatStats.OnDisable();
            PandoraSingleton<UIMissionManager>.Instance.StateMachine.ChangeState(1);
        }
    }

    private void SelectCurrentDestructible(Destructible currentDestructible)
    {
        PandoraSingleton<UIMissionManager>.Instance.SetPropsInfo(currentDestructible.Imprint.visibleTexture, currentDestructible.LocalizedName);
    }

    private void EnterOrthoCam()
    {
        mngr.transitionCam = false;
        if (PandoraSingleton<MissionManager>.Instance.mapContour != null)
        {
            mapBounds = default(Bounds);
            float num = 100f;
            for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.mapContour.FlatEdges.Count; i++)
            {
                Vector2 item = PandoraSingleton<MissionManager>.Instance.mapContour.FlatEdges[i].Item1;
                mapBounds.Encapsulate(new Vector3(item.x, (float)(i % 2) * num, item.y));
            }
            mapBounds.SetMinMax(mapBounds.min, mapBounds.max + new Vector3(0f, 0f, 15f));
        }
        mngr.SetDOFActive(active: false);
        cam.orthographic = true;
        cam.orthographicSize = zoomLevels[zoomIdx];
        Vector3 position = cam.transform.position + -cam.transform.forward * 40f;
        cam.transform.position = position;
        dummyCam.position = cam.transform.position;
    }

    private void ExitOrthoCam()
    {
        cam.orthographic = false;
        mngr.SetDOFActive(active: true);
        mngr.transitionCam = true;
    }

    public void SetTarget(Transform target, bool immediate = false)
    {
        if ((bool)target)
        {
            lookAtTarget.rotation = target.rotation;
            SetTarget(target.position);
        }
    }

    public void SetTarget(Vector3 targetPos, bool immediate = false)
    {
        PandoraSingleton<UIMissionManager>.Instance.HideUnitStats();
        lookAtTargetTargetPosition = targetPos;
        if (immediate)
        {
            lookAtTarget.transform.position = targetPos;
        }
        dummyCam.position = targetPos;
    }

    private void SetTargetZoom()
    {
        targetZoom = zoomLevels[zoomIdx];
        oldZoom = currentZoom;
        RefreshUI();
    }

    private void SetTargetRotation()
    {
        targetRotation = (float)rotationIdx * rotationAngle;
        oldRotation = currentRotation;
    }

    public void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
    {
        PandoraSingleton<MissionManager>.Instance.CamManager.RemoveLOSLayer("characters");
        Vector3 a = OffsetPosition(lookAtTarget, new Vector3(0f, 10f, -5f)) - OffsetPosition(lookAtTarget, new Vector3(0f, 0f, -3f));
        a.Normalize();
        position = OffsetPosition(lookAtTarget, new Vector3(0f, 0f, -3f)) + a * currentZoom;
        angle = Quaternion.LookRotation(lookAtTarget.position + offsetVector - position);
    }

    private Vector3 OffsetPosition(Transform trans, Vector3 offset)
    {
        Vector3 position = trans.position;
        position += trans.forward * offset.z;
        position += trans.up * offset.y;
        return position + trans.right * offset.x;
    }

    public Transform GetTarget()
    {
        return lookAtTarget;
    }

    public void SetSelectedOverview(int idx)
    {
        for (int i = 0; i < flyingOverviews.Count; i++)
        {
            if (flyingOverviews[i] != null && ((Behaviour)(object)flyingOverviews[i].bgSelected).enabled != (i == idx))
            {
                ((Behaviour)(object)flyingOverviews[i].bgSelected).enabled = (i == idx);
            }
        }
    }
}
