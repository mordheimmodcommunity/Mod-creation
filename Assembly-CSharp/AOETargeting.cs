using System.Collections.Generic;
using UnityEngine;

public class AOETargeting : ICheapState
{
    private UnitController unitCtrlr;

    private CameraManager camMngr;

    private int maxRange;

    private float sphereHeight;

    private float sphereMaxXZMagnitude;

    private Vector3 sphereRaySrc;

    private Vector3 sphereDir;

    private Vector3 prevSpherePos;

    private float sphereDist;

    private bool stickToGround;

    private float rangeMin;

    private bool isPositionValid;

    private bool needWalkable;

    private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

    public AOETargeting(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        isPositionValid = true;
        rangeMin = unitCtrlr.CurrentAction.RangeMin;
        needWalkable = unitCtrlr.CurrentAction.skillData.NeedValidGround;
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.defenders.Clear();
        unitCtrlr.destructTargets.Clear();
        availableTargets = unitCtrlr.GetCurrentActionTargets();
        maxRange = unitCtrlr.CurrentAction.RangeMax;
        sphereDist = unitCtrlr.CurrentAction.RangeMax;
        camMngr = PandoraSingleton<MissionManager>.Instance.CamManager;
        if (maxRange > 0)
        {
            camMngr.SwitchToCam(CameraManager.CameraType.FIXED, unitCtrlr.transform);
        }
        else
        {
            camMngr.SwitchToCam(CameraManager.CameraType.ROTATE_AROUND, unitCtrlr.transform);
            RotateAroundCam currentCam = camMngr.GetCurrentCam<RotateAroundCam>();
            currentCam.distance = (float)unitCtrlr.CurrentAction.Radius + 6f;
        }
        PandoraSingleton<MissionManager>.Instance.InitSphereTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.TargetingId, out sphereRaySrc, out sphereDir);
        stickToGround = (unitCtrlr.CurrentAction.TargetingId == TargetingId.AREA_GROUND);
        if (stickToGround)
        {
            sphereDir = Vector3.Normalize(unitCtrlr.transform.forward);
        }
        PandoraSingleton<MissionManager>.Instance.sphereTarget.SetActive(value: true);
        SetSpherePos();
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
        }
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<UIMissionManager>.Instance.interactiveMessage.Hide();
        unitCtrlr.ClearFlyingTexts();
        PandoraSingleton<MissionManager>.Instance.sphereTarget.SetActive(value: false);
        camMngr.ClearLookAtFocus();
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CANCEL);
            unitCtrlr.StateMachine.ChangeState((!unitCtrlr.Engaged) ? 11 : 12);
            return;
        }
        if (isPositionValid && PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
            unitCtrlr.SendSkillTargets(unitCtrlr.CurrentAction.SkillId, PandoraSingleton<MissionManager>.Instance.sphereTarget.transform.position, PandoraSingleton<MissionManager>.Instance.sphereTarget.transform.position - unitCtrlr.transform.position);
            return;
        }
        float num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
        float num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
        num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
        num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
        float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("v");
        if (stickToGround)
        {
            if (num != 0f)
            {
                Quaternion rotation = Quaternion.AngleAxis(num, Vector3.up);
                Vector3 to = rotation * sphereDir;
                if (Vector3.Angle(unitCtrlr.transform.forward, to) < 90f)
                {
                    sphereDir = to;
                }
            }
            sphereDist += (0f - num2) / 4f + axis;
        }
        else
        {
            if (num != 0f || num2 != 0f)
            {
                Quaternion lhs = Quaternion.AngleAxis(num, Vector3.up);
                Quaternion rhs = Quaternion.AngleAxis(num2, unitCtrlr.transform.right);
                Vector3 to2 = lhs * rhs * sphereDir;
                if (Vector3.Angle(unitCtrlr.transform.forward, to2) < 90f)
                {
                    sphereDir = to2;
                }
            }
            sphereDist += axis;
        }
        SetSpherePos();
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void SetSpherePos()
    {
        bool flag = true;
        Transform transform = PandoraSingleton<MissionManager>.Instance.sphereTarget.transform;
        sphereDist = Mathf.Clamp(sphereDist, 0f, maxRange);
        float distance = sphereDist;
        Ray ray = new Ray(sphereRaySrc, sphereDir);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, sphereDist, LayerMaskManager.groundMask))
        {
            distance = hitInfo.distance;
        }
        transform.position = sphereRaySrc + sphereDir * distance;
        if (stickToGround)
        {
            Vector3 origin = transform.position + Vector3.up * 0.05f;
            ray = new Ray(origin, Vector3.down);
            if (Physics.Raycast(ray, out hitInfo, 100f, LayerMaskManager.groundMask))
            {
                transform.position = hitInfo.point + Vector3.up * 0.05f;
            }
            else
            {
                PandoraDebug.LogWarning("No ground found when trying to stick aoe targeting sphere.", "TARGETING", unitCtrlr);
            }
        }
        unitCtrlr.FaceTarget(transform, force: true);
        if (maxRange > 0)
        {
            camMngr.SetShoulderCam(unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
            Vector3 position = camMngr.dummyCam.transform.position;
            Vector3 position2 = unitCtrlr.transform.position;
            position.x = position2.x;
            Vector3 position3 = unitCtrlr.transform.position;
            position.z = position3.z;
            camMngr.dummyCam.transform.LookAt(transform);
            Vector3 a = camMngr.dummyCam.transform.position - transform.position;
            float num = Vector3.SqrMagnitude(a);
            if (num < ((float)unitCtrlr.CurrentAction.Radius + 3.5f) * ((float)unitCtrlr.CurrentAction.Radius + 3.5f))
            {
                camMngr.dummyCam.transform.position = transform.position + a.normalized * ((float)unitCtrlr.CurrentAction.Radius + 5f);
                if (Physics.SphereCast(position, 0.2f, -camMngr.dummyCam.transform.forward, out hitInfo, (float)unitCtrlr.CurrentAction.Radius + 4f, LayerMaskManager.groundMask))
                {
                    camMngr.dummyCam.transform.position = hitInfo.point + camMngr.dummyCam.transform.forward * 0.2f;
                }
            }
        }
        if (Vector3.SqrMagnitude(transform.position - prevSpherePos) > 4f)
        {
            camMngr.Transition();
        }
        if (!stickToGround)
        {
            unitCtrlr.SetAoeTargets(availableTargets, transform.transform, highlightTargets: true);
        }
        else
        {
            unitCtrlr.SetCylinderTargets(availableTargets, transform.transform, highlighTargets: true);
        }
        prevSpherePos = transform.position;
        if (rangeMin > 0f)
        {
            flag &= (Vector3.SqrMagnitude(transform.position - unitCtrlr.transform.position) > rangeMin * rangeMin);
        }
        if (flag && needWalkable)
        {
            flag &= PandoraSingleton<MissionManager>.Instance.IsOnNavmesh(transform.position);
            if (flag)
            {
                List<UnitController> aliveAllies = PandoraSingleton<MissionManager>.Instance.GetAliveAllies(unitCtrlr.GetWarband().idx);
                Vector3 position4 = unitCtrlr.transform.position;
                float x = position4.x;
                Vector3 position5 = unitCtrlr.transform.position;
                Vector2 point = new Vector2(x, position5.z);
                Vector3 position6 = transform.position;
                float x2 = position6.x;
                Vector3 position7 = transform.position;
                Vector2 checkDestPoint = new Vector2(x2, position7.z);
                for (int i = 0; i < aliveAllies.Count; i++)
                {
                    Vector3 position8 = aliveAllies[i].transform.position;
                    if (aliveAllies[i] != unitCtrlr)
                    {
                        float y = position8.y;
                        Vector3 position9 = transform.position;
                        if (Mathf.Abs(y - position9.y) < 1.9f && PandoraUtils.IsPointInsideEdges(aliveAllies[i].combatCircle.Edges, point, checkDestPoint))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                List<ActionZone> accessibleActionZones = PandoraSingleton<MissionManager>.Instance.accessibleActionZones;
                for (int j = 0; j < accessibleActionZones.Count; j++)
                {
                    List<UnitActionId> unitActionIds = accessibleActionZones[j].GetUnitActionIds(unitCtrlr);
                    if (unitActionIds.Count <= 0 || (unitActionIds[0] != UnitActionId.CLIMB && unitActionIds[0] != UnitActionId.CLIMB_3M && unitActionIds[0] != UnitActionId.CLIMB_6M && unitActionIds[0] != UnitActionId.CLIMB_9M && unitActionIds[0] != UnitActionId.JUMP && unitActionIds[0] != UnitActionId.JUMP_3M && unitActionIds[0] != UnitActionId.JUMP_6M && unitActionIds[0] != UnitActionId.JUMP_9M && unitActionIds[0] != UnitActionId.LEAP))
                    {
                        continue;
                    }
                    float num2 = Vector3.Dot(Vector3.Normalize(transform.position - (accessibleActionZones[j].transform.position + accessibleActionZones[j].transform.forward)), accessibleActionZones[j].transform.forward);
                    if (num2 < 0f)
                    {
                        float num3 = Vector3.SqrMagnitude(transform.position - accessibleActionZones[j].transform.position);
                        if (num3 < 4f)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
        }
        if (flag != isPositionValid)
        {
            isPositionValid = flag;
            transform.GetChild(1).gameObject.SetActive(isPositionValid);
            transform.GetChild(2).gameObject.SetActive(isPositionValid);
            transform.GetChild(3).gameObject.SetActive(isPositionValid);
            if (isPositionValid)
            {
                PandoraSingleton<UIMissionManager>.Instance.interactiveMessage.Hide();
            }
            else
            {
                PandoraSingleton<UIMissionManager>.Instance.interactiveMessage.Show(PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_action_area_blocked"));
            }
        }
    }
}
