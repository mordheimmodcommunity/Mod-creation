using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionLoader : PandoraSingleton<MissionLoader>
{
    private const int totalParts = 353;

    [HideInInspector]
    public GameObject ground;

    public int trapCount;

    private MissionSave missionSave;

    private MissionMapLayoutData mapLayout;

    private MissionMapGameplayData mapGameplay;

    private DeploymentScenarioMapLayoutData deployMapData;

    private List<PropRestrictions> restrictions;

    private List<ActionZone> actionZones = new List<ActionZone>();

    private List<SpawnZone> deployedZones;

    private List<SpawnNode> spawnNodes;

    private List<SearchZone> searchZones;

    private List<SearchNode> allSearchNodes;

    private List<SearchNode> searchNodes;

    private List<PropNode> totPropNodes;

    private List<BuildingNode> totBuildingNodes;

    private List<Trap> traps;

    private List<KeyValuePair<TrapTypeData, bool>> trapData;

    private List<TrapNode> trapNodes;

    private List<WyrdstoneTypeData> wyrdTypeData;

    private List<SearchDensityLootData> rewards;

    private List<GameObject> groundLayers;

    private List<string> groundLayersOrder;

    private Tyche networkTyche;

    private int lowestWarbandRank;

    private int[] linkCount;

    private int[] linkedCount;

    private List<string> jobs;

    private List<KeyValuePair<Transform, Transform>> parents;

    private List<int> rotations;

    private List<PropNode> tempPropNodes;

    private List<BuildingNode> tempBuildingNodes;

    private Dictionary<string, GameObject> assetLibrary;

    private string currentAssetbundleName;

    private int loadingPlanks;

    private float startTime;

    private readonly int[] LOADING_PARTS = new int[16]
    {
        10,
        50,
        50,
        5,
        50,
        5,
        5,
        50,
        10,
        5,
        5,
        1,
        100,
        5,
        1,
        1
    };

    private int currentParts;

    private float currentPartsPercent;

    private int loadingPartsIndex;

    private int loadingZoneAoe;

    public int percent;

    private int jobLoaded;

    private readonly List<ActionNode> aNodes = new List<ActionNode>();

    private int wagonQueued;

    private int loadingTraps;

    private int CurrentParts
    {
        get
        {
            return currentParts;
        }
        set
        {
            currentParts = value;
            UpdateLoadingProgress();
        }
    }

    private float CurrentPartsPercent
    {
        get
        {
            return currentPartsPercent;
        }
        set
        {
            currentPartsPercent = value;
            UpdateLoadingProgress();
        }
    }

    private void Awake()
    {
        PandoraSingleton<MissionLoader>.instance = this;
    }

    private void Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.High;
        totPropNodes = new List<PropNode>();
        totBuildingNodes = new List<BuildingNode>();
        jobs = new List<string>();
        parents = new List<KeyValuePair<Transform, Transform>>();
        rotations = new List<int>();
        tempPropNodes = new List<PropNode>();
        tempBuildingNodes = new List<BuildingNode>();
        assetLibrary = new Dictionary<string, GameObject>();
        groundLayers = new List<GameObject>(11);
        groundLayersOrder = new List<string>(11);
        startTime = Time.realtimeSinceStartup;
        loadingPlanks = 0;
        loadingZoneAoe = 0;
        PandoraSingleton<MissionManager>.Instance.CamManager.GetComponent<Camera>().enabled = false;
        PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.FIXED, null, transition: false, force: true);
        PandoraSingleton<MissionManager>.Instance.CamManager.dummyCam.transform.position = new Vector3(1000f, 1000f, 1000f);
        PandoraSingleton<Hermes>.Instance.DoNotDisconnectMode = true;
        LoadMissionData();
        StartCoroutine(GenerateAllAsync());
    }

    private void UpdateLoadingProgress()
    {
        if (loadingPartsIndex >= LOADING_PARTS.Length)
        {
            percent = 100;
        }
        else
        {
            percent = Mathf.FloorToInt(((float)CurrentParts + CurrentPartsPercent * (float)LOADING_PARTS[loadingPartsIndex]) / 353f * 100f);
        }
    }

    public IEnumerator GenerateAllAsync()
    {
        percent = 0;
        CurrentParts = 0;
        loadingPartsIndex = 0;
        GenerateLayersAsync();
        yield return StartCoroutine(WaitLayers());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        currentAssetbundleName = AssetBundleId.BUILDINGS_SCENE.ToLowerString();
        GenerateBuildingsAsync(ground);
        yield return StartCoroutine(CheckSceneJobs(OnBuildingLoaded, null));
        PandoraSingleton<AssetBundleLoader>.Instance.Unload(currentAssetbundleName);
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        currentAssetbundleName = AssetBundleId.PROPS_SCENE.ToLowerString();
        GeneratePropsAsync(ground);
        yield return StartCoroutine(CheckSceneJobs(OnPropLoaded, null));
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        InitActionNodes();
        GenerateHangingNodes();
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        while (loadingPlanks > 0)
        {
            yield return null;
        }
        PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
        GenerateRestrictedPropsAsync();
        yield return StartCoroutine(CheckSceneJobs(OnPropLoaded, null));
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        StartCoroutine(GenerateUnits());
        DestroyBuildingPropNodes();
        yield return StartCoroutine(BatchEnvironment());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        InitActionNodes();
        yield return StartCoroutine(CreateActionWeb());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        while (PandoraSingleton<UnitFactory>.instance.IsCreating())
        {
            CurrentPartsPercent = Mathf.Max(CurrentPartsPercent, PandoraSingleton<UnitFactory>.Instance.LoadingPercent);
            yield return null;
        }
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
        GenerateTrapsAsync();
        yield return StartCoroutine(CheckSceneJobs(OnTrapLoaded, OnTrapCleared));
        TrapsPostProcess();
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        GenerateWyrdStonesAsync();
        yield return StartCoroutine(CheckSceneJobs(OnWyrdstoneLoaded, OnWyrdstoneCleared));
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        GenerateSearchAsync();
        yield return StartCoroutine(CheckSceneJobs(OnSearchLoaded, null));
        SearchPostProcess();
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        GenerateReinforcements();
        while (PandoraSingleton<UnitFactory>.instance.IsCreating())
        {
            yield return null;
        }
        yield return StartCoroutine(DeployUnits());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        yield return StartCoroutine(GenerateNavMesh());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        SetCameraSetter();
        if (PandoraSingleton<MissionStartData>.Instance.isReload)
        {
            MissionReloadPostProcess();
        }
        while (loadingZoneAoe > 0)
        {
            yield return null;
        }
        yield return StartCoroutine(ReloadTraps());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        yield return StartCoroutine(ClearLoadingElements());
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        SetupObjectives();
        ReloadDestructibles();
        CurrentPartsPercent = 0f;
        CurrentParts += LOADING_PARTS[loadingPartsIndex++];
        yield return null;
        PandoraSingleton<MissionManager>.Instance.SendLoadingDone();
        Debug.LogError("Loading Done! Total Time :" + (Time.realtimeSinceStartup - startTime) + "s");
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        UnityEngine.Object.Destroy(this);
    }

    private void InitActionNodes()
    {
        aNodes.Clear();
        ground.GetComponentsInChildren(aNodes);
        for (int i = 0; i < aNodes.Count; i++)
        {
            aNodes[i].Init();
        }
    }

    private void MissionReloadPostProcess()
    {
        List<KeyValuePair<uint, SearchSave>> searches = PandoraSingleton<MissionStartData>.Instance.searches;
        List<InteractivePoint> interactivePoints = PandoraSingleton<MissionManager>.Instance.interactivePoints;
        PandoraDebug.LogWarning("Interactive Points counts " + interactivePoints.Count);
        for (int i = 0; i < searches.Count; i++)
        {
            bool flag = false;
            for (int j = 0; j < interactivePoints.Count; j++)
            {
                if (searches[i].Key >= 200000000 || interactivePoints[j].guid != searches[i].Key)
                {
                    continue;
                }
                SearchPoint searchPoint = (SearchPoint)interactivePoints[j];
                PandoraDebug.LogDebug("Search Point Name = " + searchPoint.name);
                searchPoint.GetItemsAndClear();
                if (searches[i].Value != null)
                {
                    for (int k = 0; k < searches[i].Value.items.Count; k++)
                    {
                        if (k >= searchPoint.items.Count)
                        {
                            searchPoint.AddItem(searches[i].Value.items[k]);
                        }
                        else
                        {
                            searchPoint.SetItem(searches[i].Value.items[k], k);
                        }
                        PandoraSingleton<MissionManager>.Instance.ResetItemOwnership(searchPoint.items[k], null);
                    }
                    searchPoint.wasSearched = searches[i].Value.wasSearched;
                }
                searchPoint.Refresh();
                searchPoint.Close(force: true);
                flag = true;
                break;
            }
            if (!flag && searches[i].Value.items != null)
            {
                SearchPoint searchPoint2 = PandoraSingleton<MissionManager>.Instance.SpawnLootBag(PandoraSingleton<MissionManager>.Instance.GetUnitController(searches[i].Value.unitCtrlrUid), searches[i].Value.pos, searches[i].Value.items, visible: true, searches[i].Value.wasSearched);
            }
        }
        List<KeyValuePair<uint, int>> converters = PandoraSingleton<MissionStartData>.Instance.converters;
        for (int l = 0; l < converters.Count; l++)
        {
            for (int m = 0; m < interactivePoints.Count; m++)
            {
                if (interactivePoints[m].guid == converters[l].Key)
                {
                    ((ConvertPoint)interactivePoints[m]).SetCapacity(converters[l].Value);
                    ((ConvertPoint)interactivePoints[m]).Close();
                }
            }
        }
        List<KeyValuePair<uint, bool>> activaters = PandoraSingleton<MissionStartData>.Instance.activaters;
        for (int n = 0; n < activaters.Count; n++)
        {
            for (int num = 0; num < interactivePoints.Count; num++)
            {
                if (interactivePoints[num].guid == activaters[n].Key)
                {
                    ((ActivatePoint)interactivePoints[num]).activated = !activaters[n].Value;
                    ((ActivatePoint)interactivePoints[num]).ActivateZoneAoe();
                    ((ActivatePoint)interactivePoints[num]).Activate(null, force: true);
                }
            }
        }
        List<EndZoneAoe> zones = PandoraSingleton<MissionStartData>.Instance.aoeZones;
        for (int num2 = 0; num2 < zones.Count; num2++)
        {
            int zoneIndex = num2;
            EndZoneAoe endZoneAoe = zones[zoneIndex];
            List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
            UnitController unit = null;
            for (int num3 = 0; num3 < allUnits.Count; num3++)
            {
                if (allUnits[num3].uid == endZoneAoe.myrtilusId)
                {
                    unit = allUnits[num3];
                }
            }
            loadingZoneAoe++;
            PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/zone_aoe/", AssetBundleId.FX, endZoneAoe.aoeId.ToLowerString() + ".prefab", delegate (UnityEngine.Object prefab)
            {
                EndZoneAoe value = zones[zoneIndex];
                GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)prefab);
                gameObject.transform.position = value.position;
                gameObject.transform.rotation = Quaternion.identity;
                ZoneAoe component = gameObject.GetComponent<ZoneAoe>();
                component.Init(value.aoeId, unit, value.radius);
                component.durationLeft = value.durationLeft;
                value.guid = component.guid;
                zones[zoneIndex] = value;
                loadingZoneAoe--;
            });
        }
        List<UnitController> allUnits2 = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
        for (int num4 = 0; num4 < allUnits2.Count; num4++)
        {
            List<Item> items = allUnits2[num4].unit.Items;
            for (int num5 = 0; num5 < items.Count; num5++)
            {
                PandoraSingleton<MissionManager>.Instance.ResetItemOwnership(items[num5], allUnits2[num4]);
            }
        }
    }

    private IEnumerator BatchEnvironment()
    {
        MeshBatcher[] batchers = ground.GetComponentsInChildren<MeshBatcher>();
        LODGroup.crossFadeAnimationDuration = 1.5f;
        float st = Time.realtimeSinceStartup;
        for (int i = 0; i < batchers.Length; i++)
        {
            batchers[i].Batch();
            if ((double)(Time.realtimeSinceStartup - st) > 0.01)
            {
                yield return null;
            }
            st = Time.realtimeSinceStartup;
            CurrentPartsPercent = (float)i / (float)batchers.Length;
        }
    }

    private IEnumerator CheckSceneJobs(Action<GameObject, int> loaded, Action clear)
    {
        int i = 0;
        while (i < jobs.Count)
        {
            GameObject go = assetLibrary[jobs[i]];
            if (go != null)
            {
                GameObject obj = InstantiateAsset(parents[i].Key, parents[i].Value, rotations[i], go);
                loaded?.Invoke(obj, i);
                i++;
            }
            else
            {
                yield return null;
            }
            CurrentPartsPercent = Mathf.Max(CurrentPartsPercent, (float)(jobLoaded + 1) / (float)assetLibrary.Count);
        }
        PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
        jobs.Clear();
        parents.Clear();
        rotations.Clear();
        assetLibrary.Clear();
        clear?.Invoke();
        jobLoaded = 0;
    }

    private void OnBuildingLoaded(GameObject go, int index)
    {
        GenerateBuildingsAsync(go);
    }

    private void OnPropLoaded(GameObject go, int index)
    {
        GeneratePropsAsync(go);
    }

    private void OnTrapLoaded(GameObject go, int index)
    {
        Trap component = go.GetComponent<Trap>();
        component.Init(trapData[index].Key, PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        component.forceInactive = trapData[index].Value;
        traps.Add(component);
    }

    private void OnTrapCleared()
    {
        trapData.Clear();
    }

    private void OnWyrdstoneLoaded(GameObject go, int index)
    {
        SearchPoint component = go.GetComponent<SearchPoint>();
        component.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID(), 0, wyrdStone: true);
        component.AddItem(wyrdTypeData[index].ItemId, ItemQualityId.NORMAL);
        if (go.name.Contains("outdoor"))
        {
            component.slots[0].restrictedItemId = wyrdTypeData[index].ItemId;
        }
    }

    private void OnWyrdstoneCleared()
    {
        wyrdTypeData.Clear();
    }

    private void OnSearchLoaded(GameObject go, int index)
    {
        SearchPoint componentInChildren = go.GetComponentInChildren<SearchPoint>();
        SearchDensityLootData randomRatio = SearchDensityLootData.GetRandomRatio(rewards, networkTyche);
        int num = networkTyche.Rand(randomRatio.ItemMin, randomRatio.ItemMax + 1);
        componentInChildren.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID(), num);
        FillSearchPoint(componentInChildren, randomRatio, lowestWarbandRank, num);
    }

    public void GenerateBuildingsAsync(GameObject go)
    {
        go.GetComponentsInChildren(tempBuildingNodes);
        if (tempBuildingNodes != null)
        {
            totBuildingNodes.AddRange(tempBuildingNodes);
            for (int i = 0; i < tempBuildingNodes.Count; i++)
            {
                DataFactory instance = PandoraSingleton<DataFactory>.Instance;
                int buildingType = (int)tempBuildingNodes[i].buildingType;
                List<BuildingTypeJoinBuildingData> datas = instance.InitData<BuildingTypeJoinBuildingData>("fk_building_type_id", buildingType.ToString());
                BuildingTypeJoinBuildingData randomRatio = BuildingTypeJoinBuildingData.GetRandomRatio(datas, networkTyche);
                string assetName = randomRatio.BuildingId.ToLowerString();
                AddSceneJob(assetName, tempBuildingNodes[i].transform, randomRatio.Flippable ? 180 : 0);
            }
            tempBuildingNodes.Clear();
        }
    }

    private GameObject InstantiateAsset(Transform parent, Transform pos, int rot, GameObject asset, bool instantiate = true)
    {
        GameObject gameObject = null;
        Vector3 vector = Vector3.zero;
        Quaternion quaternion = Quaternion.identity;
        Vector3 vector2 = Vector3.one;
        if (asset != null)
        {
            if (pos != null)
            {
                Quaternion rhs = Quaternion.Euler(0f, GetRandomRotation(rot), 0f);
                vector = pos.localPosition;
                quaternion = pos.localRotation * rhs;
                vector2 = pos.localScale;
            }
            if (instantiate)
            {
                gameObject = (GameObject)UnityEngine.Object.Instantiate(asset, vector, quaternion);
            }
            else
            {
                gameObject = asset;
                gameObject.transform.localPosition = vector;
                gameObject.transform.localRotation = quaternion;
            }
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, worldPositionStays: false);
            }
            if (!PandoraUtils.Approximately(vector2, Vector3.one))
            {
                gameObject.transform.localScale = vector2;
            }
            gameObject.isStatic = true;
            gameObject.SetActive(value: true);
        }
        return gameObject;
    }

    private int GetRandomRotation(int rot)
    {
        int num = 0;
        if (rot != 0)
        {
            num = 360 / rot;
        }
        return networkTyche.Rand(0, num + 1) * rot;
    }

    private void GenerateLayersAsync()
    {
        MissionMapData missionMapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deployMapData.MissionMapId);
        string name = missionMapData.Name;
        currentAssetbundleName = name + "_scene";
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAsync(currentAssetbundleName);
        LoadLayerAsync(name, currentAssetbundleName);
        LoadLayerAsync(mapLayout.LightsName, currentAssetbundleName);
        LoadLayerAsync(mapLayout.FxName, currentAssetbundleName);
        if (missionMapData.HasRecastHelper)
        {
            string layerName = name + "_recast_helper";
            LoadLayerAsync(layerName, currentAssetbundleName);
        }
        LoadLayerAsync(deployMapData.PropsLayer, currentAssetbundleName);
        LoadLayerAsync(deployMapData.DeploymentLayer, currentAssetbundleName);
        LoadLayerAsync(deployMapData.TrapsLayer, currentAssetbundleName);
        LoadLayerAsync(deployMapData.SearchLayer, currentAssetbundleName);
        if (!string.IsNullOrEmpty(deployMapData.ExtraLightsFxLayer))
        {
            LoadLayerAsync(deployMapData.ExtraLightsFxLayer, currentAssetbundleName);
        }
        if (mapGameplay.Id != 0)
        {
            LoadLayerAsync(mapGameplay.Name, currentAssetbundleName);
        }
        if (mapLayout.CloudsName.Contains("dis_00"))
        {
            LoadLayerAsync(mapLayout.CloudsName, "grnd_dis_00_proc_00_scene");
        }
        else
        {
            LoadLayerAsync(mapLayout.CloudsName, currentAssetbundleName);
        }
    }

    private void LoadLayerAsync(string layerName, string assetBundle)
    {
        groundLayersOrder.Add(layerName);
        PandoraSingleton<AssetBundleLoader>.Instance.LoadSceneAssetAsync(layerName, assetBundle, delegate (UnityEngine.Object go)
        {
            GameObject gameObject = (GameObject)go;
            gameObject = SceneManager.GetSceneByName(layerName).GetRootGameObjects()[0];
            groundLayers.Add(gameObject);
        });
    }

    private IEnumerator WaitLayers()
    {
        while (groundLayers.Count != groundLayersOrder.Count)
        {
            CurrentPartsPercent = (float)groundLayers.Count / (float)groundLayersOrder.Count;
            yield return null;
        }
        for (int k = groundLayers.Count - 1; k >= 0; k--)
        {
            if (groundLayers[k].name == groundLayersOrder[0])
            {
                ground = groundLayers[k];
                SceneManager.MoveGameObjectToScene(ground, SceneManager.GetActiveScene());
                groundLayers.RemoveAt(k);
                break;
            }
        }
        for (int j = 1; j < groundLayersOrder.Count; j++)
        {
            for (int i = 0; i < groundLayers.Count; i++)
            {
                if (groundLayersOrder[j] == groundLayers[i].name)
                {
                    groundLayers[i].transform.SetParent(ground.transform);
                    groundLayers.RemoveAt(i);
                    break;
                }
            }
        }
        PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
        groundLayersOrder.Clear();
        groundLayers.Clear();
        PandoraSingleton<MissionManager>.Instance.mapContour = ground.GetComponentInChildren<MapContour>();
        PandoraSingleton<MissionManager>.Instance.mapOrigin = ground.GetComponentInChildren<MapOrigin>();
        PandoraSingleton<AssetBundleLoader>.Instance.Unload("grnd_dis_00_proc_00_scene");
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAsync(currentAssetbundleName);
    }

    private PropData GetRandomProp(PropTypeId propTypeId)
    {
        List<PropTypeJoinPropData> list = PandoraSingleton<DataFactory>.Instance.InitData<PropTypeJoinPropData>("fk_prop_type_id", propTypeId.ToIntString());
        return PandoraSingleton<DataFactory>.Instance.InitData<PropData>((int)list[networkTyche.Rand(0, list.Count)].PropId);
    }

    private void GeneratePropsAsync(GameObject parent)
    {
        parent.GetComponentsInChildren(includeInactive: true, tempPropNodes);
        totPropNodes.AddRange(tempPropNodes);
        for (int i = 0; i < tempPropNodes.Count; i++)
        {
            if (!AddIfRestricted(tempPropNodes[i]))
            {
                PropTypeData propTypeData = PandoraSingleton<DataFactory>.Instance.InitData<PropTypeData>((int)tempPropNodes[i].propType);
                PropData randomProp = GetRandomProp(propTypeData.Id);
                AddSceneJob(randomProp.Name, tempPropNodes[i].transform);
            }
        }
        tempPropNodes.Clear();
    }

    private void AddSceneJob(string assetName, Transform nodeTransform, int rotation = 0)
    {
        AddSceneJob(assetName, nodeTransform.parent, nodeTransform, rotation);
    }

    private void AddSceneJob(string assetName, Transform parentTransform, Transform nodeTransform, int rotation = 0)
    {
        if (!assetLibrary.ContainsKey(assetName))
        {
            assetLibrary[assetName] = null;
            PandoraSingleton<AssetBundleLoader>.Instance.LoadSceneAssetAsync(assetName, currentAssetbundleName, delegate (UnityEngine.Object prefab)
            {
                GameObject gameObject = (GameObject)prefab;
                Scene sceneByName = SceneManager.GetSceneByName(assetName);
                if (0 == 0 && sceneByName.IsValid())
                {
                    gameObject = sceneByName.GetRootGameObjects()[0];
                    assetLibrary[assetName] = gameObject;
                    jobLoaded++;
                }
            });
        }
        jobs.Add(assetName);
        parents.Add(new KeyValuePair<Transform, Transform>(parentTransform, nodeTransform));
        rotations.Add(rotation);
    }

    private void GenerateRestrictedPropsAsync()
    {
        if (restrictions == null)
        {
            return;
        }
        for (int i = 0; i < restrictions.Count; i++)
        {
            PropRestrictions propRestrictions = restrictions[i];
            if (propRestrictions.props.Count <= 0)
            {
                continue;
            }
            float num = propRestrictions.restrictionData.MinDistance * propRestrictions.restrictionData.MinDistance;
            int count = propRestrictions.props.Count;
            List<Transform> list = new List<Transform>();
            float num2 = list.Count * 100 / count;
            int maxProp = propRestrictions.restrictionData.MaxProp;
            int maxPercentage = propRestrictions.restrictionData.MaxPercentage;
            while (propRestrictions.props.Count > 0 && list.Count < maxProp && num2 < (float)maxPercentage)
            {
                int index = networkTyche.Rand(0, propRestrictions.props.Count);
                if (propRestrictions.props[index] == null)
                {
                    propRestrictions.props.RemoveAt(index);
                    continue;
                }
                Transform transform = propRestrictions.props[index].transform;
                bool flag = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (flag)
                    {
                        break;
                    }
                    if (!(num > 0.1f))
                    {
                        break;
                    }
                    if (num > Vector3.SqrMagnitude(transform.position - list[j].position))
                    {
                        flag = true;
                        propRestrictions.props.RemoveAt(index);
                    }
                }
                if (!flag)
                {
                    PropTypeData propTypeData = PandoraSingleton<DataFactory>.Instance.InitData<PropTypeData>((int)propRestrictions.restrictionData.PropTypeId);
                    PropData randomProp = GetRandomProp(propTypeData.Id);
                    AddSceneJob(randomProp.Name, transform);
                    propRestrictions.props.RemoveAt(index);
                    list.Add(transform);
                    num2 = list.Count * 100 / count;
                }
            }
        }
    }

    private IEnumerator ClearLoadingElements()
    {
        while (PandoraSingleton<AssetBundleLoader>.Instance.IsLoading)
        {
            yield return null;
        }
        PandoraDebug.LogDebug("ClearLoadingElements 1", "LOADING", this);
        GameObject[] gos = GameObject.FindGameObjectsWithTag("loading");
        for (int goIndex = 0; goIndex < gos.Length; goIndex++)
        {
            UnityEngine.Object.DestroyImmediate(gos[goIndex]);
        }
        PandoraDebug.LogDebug("ClearLoadingElements 2", "LOADING", this);
        yield return StartCoroutine(PandoraSingleton<AssetBundleLoader>.Instance.UnloadAll());
        PandoraDebug.LogDebug("ClearLoadingElements Finished!", "LOADING", this);
    }

    private void LoadMissionData()
    {
        missionSave = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave;
        PandoraSingleton<MissionManager>.Instance.campaignId = (CampaignMissionId)missionSave.campaignId;
        networkTyche = PandoraSingleton<MissionManager>.Instance.NetworkTyche;
        lowestWarbandRank = -1;
        for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count; i++)
        {
            MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[i];
            if (missionWarbandSave.Rank > 0 && (lowestWarbandRank == -1 || missionWarbandSave.Rank < lowestWarbandRank))
            {
                lowestWarbandRank = missionWarbandSave.Rank;
            }
        }
        lowestWarbandRank = Mathf.Max(0, lowestWarbandRank);
        if (ground == null)
        {
            deployMapData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
            restrictions = new List<PropRestrictions>();
            AddPropsRestriction(deployMapData.PropRestrictionIdBarricade);
            AddPropsRestriction(deployMapData.PropRestrictionIdMadstuff);
            AddPropsRestriction(deployMapData.PropRestrictionIdProps);
            mapLayout = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapLayoutData>(missionSave.mapLayoutId);
            mapGameplay = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapGameplayData>(missionSave.mapGameplayId);
            trapCount = deployMapData.TrapCount;
            PandoraSingleton<MissionManager>.Instance.mapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deployMapData.MissionMapId);
        }
        PandoraSingleton<MissionManager>.Instance.SetTurnTimer(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.turnTimer);
        PandoraSingleton<MissionManager>.Instance.SetBeaconLimit(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.beaconLimit);
        if (missionSave.isCampaign)
        {
            PandoraSingleton<Hephaestus>.Instance.SetRichPresence((!missionSave.isTuto) ? Hephaestus.RichPresenceId.CAMPAIGN_MISSION : Hephaestus.RichPresenceId.TUTORIAL_MISSION, active: true);
        }
        else if (missionSave.isSkirmish)
        {
            if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.AI)
            {
                PandoraSingleton<Hephaestus>.instance.SetRichPresence(Hephaestus.RichPresenceId.EXHIBITION_AI, active: true);
            }
            else
            {
                PandoraSingleton<Hephaestus>.instance.SetRichPresence(Hephaestus.RichPresenceId.EXHIBITION_PLAYER, active: true);
            }
        }
        else if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.AI)
        {
            PandoraSingleton<Hephaestus>.instance.SetRichPresence(Hephaestus.RichPresenceId.PROC_MISSION, active: true);
        }
        else
        {
            PandoraSingleton<Hephaestus>.instance.SetRichPresence(Hephaestus.RichPresenceId.CONTEST, active: true);
        }
    }

    private void AddPropsRestriction(PropRestrictionId propsRestrictId)
    {
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)propsRestrictId;
        List<PropRestrictionJoinPropTypeData> list = instance.InitData<PropRestrictionJoinPropTypeData>("fk_prop_restriction_id", num.ToString());
        for (int i = 0; i < list.Count; i++)
        {
            PropRestrictionJoinPropTypeData data = list[i];
            restrictions.Add(new PropRestrictions(data));
        }
    }

    private List<SpawnNode> GetSpawnNodes()
    {
        List<SpawnNode> list = new List<SpawnNode>();
        list.AddRange(ground.GetComponentsInChildren<SpawnNode>());
        return list;
    }

    private bool AddIfRestricted(PropNode node)
    {
        if (restrictions != null)
        {
            for (int i = 0; i < restrictions.Count; i++)
            {
                PropRestrictions propRestrictions = restrictions[i];
                if (node.propType == propRestrictions.restrictionData.PropTypeId)
                {
                    propRestrictions.props.Add(node.gameObject);
                    return true;
                }
            }
        }
        return false;
    }

    private void GenerateHangingNodes()
    {
        HangingNode[] componentsInChildren = ground.GetComponentsInChildren<HangingNode>();
        PandoraDebug.LogInfo("Hangings Nodes = " + componentsInChildren.Length, "PROCEDURAL");
        List<int>[] array = new List<int>[componentsInChildren.Length];
        bool[] array2 = new bool[componentsInChildren.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new List<int>();
        }
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            if (array2[j])
            {
                continue;
            }
            HangingNode hangingNode = componentsInChildren[j];
            for (int k = 0; k < componentsInChildren.Length; k++)
            {
                if (!array2[k])
                {
                    HangingNode hangingNode2 = componentsInChildren[k];
                    float num = Vector3.SqrMagnitude(hangingNode2.transform.position - hangingNode.transform.position);
                    if (num < 4f && Vector3.Dot(hangingNode2.transform.forward, hangingNode.transform.forward) < -0.98f)
                    {
                        PandoraDebug.LogInfo("Skip Hanging Nodes! " + j + " pos = " + hangingNode.transform.position + " and " + k + " pos " + hangingNode2.transform.position + "Distance " + num, "PROCEDURAL");
                        array2[k] = true;
                        array2[j] = true;
                    }
                }
            }
        }
        for (int l = 0; l < componentsInChildren.Length; l++)
        {
            CurrentPartsPercent = (float)l / (float)componentsInChildren.Length;
            if (array2[l])
            {
                continue;
            }
            HangingNode hangingNode3 = componentsInChildren[l];
            for (int m = 0; m < componentsInChildren.Length; m++)
            {
                if (l == m || array2[m])
                {
                    continue;
                }
                HangingNode hangingNode4 = componentsInChildren[m];
                if (hangingNode4.isPlank != hangingNode3.isPlank)
                {
                    continue;
                }
                if (!hangingNode3.isPlank)
                {
                    bool flag = true;
                    for (int n = 0; n < array[m].Count; n++)
                    {
                        if (!flag)
                        {
                            break;
                        }
                        if (array[m][n] == l)
                        {
                            flag = false;
                        }
                    }
                    for (int num2 = 0; num2 < array[l].Count; num2++)
                    {
                        if (!flag)
                        {
                            break;
                        }
                        if (array[l][num2] == m)
                        {
                            flag = false;
                        }
                    }
                    if (!flag)
                    {
                        continue;
                    }
                }
                Vector3 vector = hangingNode4.transform.position - hangingNode3.transform.position;
                float num3 = Vector3.SqrMagnitude(vector);
                if (Vector3.SqrMagnitude(hangingNode4.transform.position - (hangingNode3.transform.position + hangingNode3.transform.forward)) > num3 || !((double)num3 > 20.25) || ((!((double)num3 < 182.25) || !hangingNode3.isPlank) && !((double)num3 < 90.25)) || !(Mathf.Abs(vector.y) < 0.5f) || !(hangingNode4.transform.forward == -hangingNode3.transform.forward))
                {
                    continue;
                }
                if (hangingNode3.isPlank)
                {
                    if (array[m].Count > 0 || array[l].Count > 0)
                    {
                        continue;
                    }
                    RaycastHit raycastHit = default(RaycastHit);
                    if (!PandoraUtils.RectCast(hangingNode3.transform.position + new Vector3(0f, 0.75f, 0f), vector, vector.magnitude + 0.5f, 1.25f, 1.25f, LayerMaskManager.groundMask, null, out raycastHit))
                    {
                        continue;
                    }
                    float num4 = Vector3.Angle(hangingNode4.transform.forward, vector);
                    if (num4 < 145f)
                    {
                        continue;
                    }
                    RaycastHit[] array3 = Physics.RaycastAll(hangingNode3.transform.position + Vector3.up * 0.5f, vector, vector.magnitude, (1 << LayerMask.NameToLayer("ignore_raycast")) | (1 << LayerMask.NameToLayer("collision_wall")));
                    if (array3.Length != 2)
                    {
                        continue;
                    }
                }
                else if (Physics.SphereCast(new Ray(hangingNode3.transform.position, vector), 0.15f, vector.magnitude, LayerMaskManager.groundMask))
                {
                    continue;
                }
                PropTypeId propTypeId = PropTypeId.GARLAND_DIS_00_5M;
                string text = null;
                float scale = 1f;
                if (hangingNode3.isPlank)
                {
                    if ((double)num3 < 56.25)
                    {
                        text = "woodplank_dis_01_6m_01";
                        if ((double)num3 > 42.25)
                        {
                            scale = 1.1f;
                        }
                    }
                    else if ((double)num3 < 110.25)
                    {
                        text = "woodplank_dis_01_9m_01";
                        if ((double)num3 > 90.25)
                        {
                            scale = 1.1f;
                        }
                    }
                    else
                    {
                        text = "woodplank_dis_01_12m_01";
                        if ((double)num3 > 156.25)
                        {
                            scale = 1.1f;
                        }
                    }
                    PandoraDebug.LogInfo("Plank Loaded = " + text, "PROCEDURAL");
                    LoadPlank(text, hangingNode3.transform.position, scale, vector);
                }
                else
                {
                    propTypeId = (((double)num3 < 30.25) ? PropTypeId.GARLAND_DIS_00_5M : (((double)num3 < 42.25) ? PropTypeId.GARLAND_DIS_00_6M : (((double)num3 < 56.25) ? PropTypeId.GARLAND_DIS_00_7M : ((!((double)num3 < 72.25)) ? PropTypeId.GARLAND_DIS_00_9M : PropTypeId.GARLAND_DIS_00_8M))));
                    PropTypeData propTypeData = PandoraSingleton<DataFactory>.Instance.InitData<PropTypeData>((int)propTypeId);
                    PropData randomProp = GetRandomProp(propTypeData.Id);
                    text = randomProp.Name;
                    PandoraDebug.LogInfo("Garland Loaded = " + text, "PROCEDURAL");
                    LoadPlank(text, hangingNode3.transform.position, 1f, vector);
                }
                array[l].Add(m);
                array[m].Add(l);
                break;
            }
        }
        for (int num5 = 0; num5 < componentsInChildren.Length; num5++)
        {
            if (componentsInChildren[num5].isPlank || array[num5].Count == 0)
            {
                UnityEngine.Object.Destroy(componentsInChildren[num5].gameObject);
            }
        }
    }

    private void LoadPlank(string goName, Vector3 position, float scale, Vector3 distV)
    {
        loadingPlanks++;
        PandoraSingleton<AssetBundleLoader>.Instance.LoadSceneAssetAsync(goName, AssetBundleId.PROPS_SCENE.ToLowerString(), delegate (UnityEngine.Object go)
        {
            loadingPlanks--;
            go = SceneManager.GetSceneByName(goName).GetRootGameObjects()[0];
            GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)go);
            gameObject.transform.position = position + distV / 2f;
            gameObject.transform.SetParent(ground.transform);
            gameObject.transform.rotation = Quaternion.FromToRotation(gameObject.transform.right, distV);
            Vector3 one = Vector3.one;
            one.x = scale;
            gameObject.transform.localScale = one;
        });
    }

    private void DestroyBuildingPropNodes()
    {
        if (totPropNodes != null)
        {
            for (int num = totPropNodes.Count - 1; num >= 0; num--)
            {
                UnityEngine.Object.DestroyImmediate(totPropNodes[num].gameObject);
            }
        }
        if (totBuildingNodes != null)
        {
            for (int num2 = totBuildingNodes.Count - 1; num2 >= 0; num2--)
            {
                UnityEngine.Object.DestroyImmediate(totBuildingNodes[num2].gameObject);
            }
        }
    }

    private IEnumerator CreateActionWeb()
    {
        ActionZone[] aZones = ground.GetComponentsInChildren<ActionZone>();
        ActionZoneChecker[] aZonesCheck = ground.GetComponentsInChildren<ActionZoneChecker>();
        for (int i = 0; i < aZones.Length; i++)
        {
            aZones[i].gameObject.SetActive(value: false);
        }
        for (int j = 0; j < aZonesCheck.Length; j++)
        {
            aZonesCheck[j].toDestroy = false;
            aZonesCheck[j].gameObject.SetActive(value: false);
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        for (int k = 0; k < aZones.Length; k++)
        {
            aZones[k].gameObject.SetActive(value: true);
        }
        for (int l = 0; l < aZonesCheck.Length; l++)
        {
            aZonesCheck[l].gameObject.SetActive(value: true);
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        if (aZones.Length > actionZones.Count)
        {
            actionZones.Clear();
            actionZones.AddRange(aZones);
        }
        for (int m = 0; m < aZonesCheck.Length; m++)
        {
            aZonesCheck[m].Check();
        }
        PandoraDebug.LogInfo("CreateActionWeb Zones = " + actionZones.Count, "PROCEDURAL");
        linkCount = new int[actionZones.Count];
        linkedCount = new int[actionZones.Count];
        for (int n = 0; n < actionZones.Count; n++)
        {
            linkCount[n] = 0;
            linkedCount[n] = 0;
        }
        for (int i2 = 0; i2 < actionZones.Count; i2++)
        {
            ActionZone myZone = actionZones[i2];
            ActionZoneChecker check2 = myZone.GetComponentInChildren<ActionZoneChecker>();
            if (check2.toDestroy)
            {
                continue;
            }
            if ((bool)myZone.largeOccupation && myZone.largeOccupation.Occupation > 0)
            {
                myZone.supportLargeUnit = false;
                UnityEngine.Object.Destroy(myZone.largeOccupation.gameObject);
                myZone.largeOccupation = null;
            }
            for (int j2 = 0; j2 < actionZones.Count; j2++)
            {
                if (i2 == j2)
                {
                    continue;
                }
                ActionZone checkZone = actionZones[j2];
                check2 = checkZone.GetComponentInChildren<ActionZoneChecker>();
                if ((bool)checkZone.largeOccupation && checkZone.largeOccupation.Occupation > 0)
                {
                    checkZone.supportLargeUnit = false;
                    UnityEngine.Object.Destroy(checkZone.largeOccupation.gameObject);
                    checkZone.largeOccupation = null;
                }
                bool pass = true;
                for (int destIndex = 0; destIndex < myZone.destinations.Count; destIndex++)
                {
                    ActionDestination destination = myZone.destinations[destIndex];
                    float dist = Vector3.SqrMagnitude(destination.destination.transform.position - checkZone.transform.position);
                    if ((double)dist < 0.0625)
                    {
                        pass = false;
                    }
                }
                if (!pass)
                {
                    continue;
                }
                Vector3 position = checkZone.transform.position;
                float y = position.y;
                Vector3 position2 = myZone.transform.position;
                float yDist = y - position2.y;
                Vector3 position3 = myZone.transform.position;
                float x = position3.x;
                Vector3 position4 = myZone.transform.position;
                Vector3 a = new Vector3(x, 0f, position4.z);
                Vector3 position5 = checkZone.transform.position;
                float x2 = position5.x;
                Vector3 position6 = checkZone.transform.position;
                float xDist = Vector3.SqrMagnitude(a - new Vector3(x2, 0f, position6.z));
                if (xDist < 3.23999977f && yDist < 1E-05f && Vector3.Dot(checkZone.transform.forward, myZone.transform.forward) < -0.98f && Vector3.SqrMagnitude(checkZone.transform.position - (myZone.transform.position + myZone.transform.forward)) < xDist)
                {
                    check2.toDestroy = true;
                    check2 = myZone.GetComponentInChildren<ActionZoneChecker>();
                    check2.toDestroy = true;
                }
                if (check2.toDestroy || xDist > 19.65f || !(yDist < -1f) || !(xDist < 3f) || !(checkZone.transform.forward == -myZone.transform.forward))
                {
                    continue;
                }
                Vector3 rayDir = checkZone.transform.position - myZone.transform.position;
                Vector3 rayDirH2 = rayDir;
                rayDirH2.y = 0f;
                Vector3 rayDirV = rayDir;
                rayDirV.z = 0f;
                rayDirV.x = 0f;
                LayerMask rayLayers2 = default(LayerMask);
                rayLayers2 = ((1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("ground")));
                RaycastHit hit = default(RaycastHit);
                bool horizontal = Physics.CapsuleCast(myZone.transform.position + new Vector3(0f, 1f, 0f), myZone.transform.position + new Vector3(0f, 1.25f, 0f), 0.5f, rayDirH2, out hit, rayDirH2.magnitude + 0.5f, rayLayers2);
                Vector3 position7 = checkZone.transform.position;
                float x3 = position7.x;
                Vector3 position8 = myZone.transform.position;
                float y2 = position8.y + 1f;
                Vector3 position9 = checkZone.transform.position;
                Vector3 point = new Vector3(x3, y2, position9.z);
                Vector3 position10 = checkZone.transform.position;
                float x4 = position10.x;
                Vector3 position11 = myZone.transform.position;
                float y3 = position11.y + 1.75f;
                Vector3 position12 = checkZone.transform.position;
                bool vertical = Physics.CapsuleCast(point, new Vector3(x4, y3, position12.z), 0.5f, rayDirV, out hit, rayDirV.magnitude + 0.5f, rayLayers2);
                if (horizontal || vertical)
                {
                    continue;
                }
                ActionDestination destination3 = new ActionDestination();
                int magnitude = Mathf.Abs((int)yDist);
                destination3.actionId = ((magnitude <= 3) ? UnitActionId.JUMP_3M : ((magnitude > 6) ? UnitActionId.JUMP_9M : UnitActionId.JUMP_6M));
                destination3.destination = checkZone;
                LoadAthleticFx(destination3, "fx_jump_01", i2);
                myZone.destinations.Add(destination3);
                linkedCount[j2]++;
                Vector3 rayPos = checkZone.transform.position;
                rayPos += Vector3.up;
                LayerMask climbLayer2 = default(LayerMask);
                climbLayer2 = 1 << LayerMask.NameToLayer("loading");
                bool canClimb = true;
                for (int climbIndex = 0; climbIndex < magnitude / 3; climbIndex++)
                {
                    if (!canClimb)
                    {
                        break;
                    }
                    Vector3 checkRay = rayPos + new Vector3(0f, climbIndex * 3, 0f);
                    canClimb = Physics.Raycast(checkRay, -rayDirH2, 2f, climbLayer2);
                }
                if (canClimb)
                {
                    destination3 = new ActionDestination
                    {
                        destination = myZone,
                        actionId = ((magnitude <= 3) ? UnitActionId.CLIMB_3M : ((magnitude > 6) ? UnitActionId.CLIMB_9M : UnitActionId.CLIMB_6M))
                    };
                    int azIndex = j2;
                    LoadAthleticFx(destination3, "fx_climb_01", j2);
                    checkZone.destinations.Add(destination3);
                    linkedCount[i2]++;
                }
            }
            linkCount[i2] = myZone.destinations.Count;
        }
        for (int i3 = 0; i3 < actionZones.Count; i3++)
        {
            if (linkCount[i3] == 0 && linkedCount[i3] == 0)
            {
                ActionZoneChecker check4 = actionZones[i3].GetComponentInChildren<ActionZoneChecker>();
                if (check4.toDestroy)
                {
                    UnityEngine.Object.Destroy(actionZones[i3].transform.parent.gameObject);
                }
                else
                {
                    UnityEngine.Object.Destroy(actionZones[i3].gameObject);
                }
                continue;
            }
            ActionZone myZone2 = actionZones[i3];
            Rigidbody[] rbs = myZone2.gameObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody body in rbs)
            {
                UnityEngine.Object.Destroy(body);
            }
            ActionZoneChecker[] checks = myZone2.gameObject.GetComponentsInChildren<ActionZoneChecker>();
            foreach (ActionZoneChecker check3 in checks)
            {
                UnityEngine.Object.Destroy(check3);
            }
            if (myZone2.largeOccupation != null)
            {
                UnityEngine.Object.Destroy(myZone2.largeOccupation.GetComponent<Rigidbody>());
            }
            bool pass2 = false;
            for (int k2 = 0; k2 < myZone2.destinations.Count; k2++)
            {
                if (myZone2.destinations[k2].actionId == UnitActionId.JUMP_3M || myZone2.destinations[k2].actionId == UnitActionId.JUMP_6M || myZone2.destinations[k2].actionId == UnitActionId.JUMP_9M)
                {
                    pass2 = true;
                    break;
                }
            }
            int j3 = 0;
            while (pass2 && j3 < actionZones.Count)
            {
                if (i3 != j3 && (linkCount[j3] != 0 || linkedCount[j3] != 0))
                {
                    ActionZone checkZone2 = actionZones[j3];
                    Vector3 position13 = checkZone2.transform.position;
                    float y4 = position13.y;
                    Vector3 position14 = myZone2.transform.position;
                    float yDist2 = y4 - position14.y;
                    Vector3 position15 = myZone2.transform.position;
                    float x5 = position15.x;
                    Vector3 position16 = myZone2.transform.position;
                    Vector3 a2 = new Vector3(x5, 0f, position16.z);
                    Vector3 position17 = checkZone2.transform.position;
                    float x6 = position17.x;
                    Vector3 position18 = checkZone2.transform.position;
                    float xDist2 = Vector3.SqrMagnitude(a2 - new Vector3(x6, 0f, position18.z));
                    if (!(xDist2 > 19.65f) && !(Vector3.SqrMagnitude(checkZone2.transform.position - (myZone2.transform.position + myZone2.transform.forward)) > xDist2) && yDist2 == 0f && xDist2 < 19.65f && xDist2 > 15.65f && checkZone2.transform.forward == -myZone2.transform.forward)
                    {
                        Vector3 rayDir2 = checkZone2.transform.position - myZone2.transform.position;
                        Vector3 rayDirH = rayDir2;
                        rayDirH.y = 0f;
                        LayerMask rayLayers4 = default(LayerMask);
                        rayLayers4 = ((1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("ground")));
                        RaycastHit hit2 = default(RaycastHit);
                        if (!Physics.CapsuleCast(myZone2.transform.position + new Vector3(0f, 1f, 0f), myZone2.transform.position + new Vector3(0f, 2f, 0f), 0.5f, rayDirH, out hit2, rayDirH.magnitude, rayLayers4))
                        {
                            ActionDestination destination4 = new ActionDestination
                            {
                                actionId = UnitActionId.LEAP,
                                destination = checkZone2
                            };
                            int acMyzoneidx = i3;
                            LoadAthleticFx(destination4, "fx_leap_01", i3);
                            myZone2.destinations.Add(destination4);
                            linkedCount[j3]++;
                        }
                    }
                }
                j3++;
            }
            linkCount[i3] = myZone2.destinations.Count;
            if (linkCount[i3] == 0)
            {
                myZone2.transform.parent.gameObject.SetActive(value: false);
            }
            else
            {
                myZone2.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
            }
        }
        GameObject linksAnchor = new GameObject("linksAnchor");
        PandoraSingleton<MissionManager>.instance.actionZones = actionZones;
        PandoraSingleton<MissionManager>.Instance.nodeLinks = new List<NodeLink2>();
        if (actionZones.Count > 0)
        {
            for (int i4 = 0; i4 < actionZones.Count; i4++)
            {
                if (linkCount[i4] > 0)
                {
                    ActionZone zone = actionZones[i4];
                    for (int destIndex2 = 0; destIndex2 < zone.destinations.Count; destIndex2++)
                    {
                        ActionDestination dest = zone.destinations[destIndex2];
                        AddNodeLink(linksAnchor, zone.transform, dest.destination.transform, dest);
                    }
                }
            }
        }
        Teleporter[] teleporters = ground.GetComponentsInChildren<Teleporter>();
        foreach (Teleporter tel in teleporters)
        {
            AddNodeLink(linksAnchor, tel.transform, tel.exit.transform, null);
        }
        DecisionPoint[] decisions = ground.GetComponentsInChildren<DecisionPoint>();
        for (int i6 = 0; i6 < decisions.Length; i6++)
        {
            decisions[i6].Register();
        }
        PatrolRoute[] patrolRoutes = ground.GetComponentsInChildren<PatrolRoute>();
        for (int i5 = 0; i5 < patrolRoutes.Length; i5++)
        {
            patrolRoutes[i5].CheckValidity();
        }
    }

    private void LoadAthleticFx(ActionDestination destination, string name, int index)
    {
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, name + ".prefab", delegate (UnityEngine.Object go)
        {
            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(go);
            gameObject.SetActive(value: true);
            gameObject.transform.SetParent(actionZones[index].transform.parent);
            gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            destination.fx = gameObject;
        });
    }

    private void AddNodeLink(GameObject anchor, Transform enterPoint, Transform exitPoint, ActionDestination dest)
    {
        GameObject gameObject = new GameObject("node_link_teleport");
        gameObject.transform.SetParent(anchor.transform);
        NodeLink2 nodeLink = gameObject.AddComponent<NodeLink2>();
        nodeLink.oneWay = true;
        nodeLink.transform.position = enterPoint.position;
        nodeLink.transform.rotation = enterPoint.rotation;
        nodeLink.end = exitPoint;
        if (dest != null)
        {
            dest.navLink = nodeLink;
        }
        PandoraSingleton<MissionManager>.Instance.nodeLinks.Add(nodeLink);
    }

    private IEnumerator GenerateUnits()
    {
        Vector3 defpos = new Vector3(500f, 150f, 500f);
        for (int j = 0; j < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count; j++)
        {
            DeploymentScenarioSlotData deployScenarData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>(missionSave.deployScenarioSlotIds[j]);
            DeploymentId deployId = deployScenarData.DeploymentId;
            MissionWarbandSave missionWar = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[j];
            WarbandController warCtrlr = new WarbandController(missionWar, deployId, j, missionSave.teams[j], (PrimaryObjectiveTypeId)missionSave.objectiveTypeIds[j], missionSave.objectiveTargets[j], missionSave.objectiveSeeds[j]);
            PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Add(warCtrlr);
            for (int i = 0; i < missionWar.Units.Count; i++)
            {
                warCtrlr.GenerateUnit(missionWar.Units[i], defpos + new Vector3((float)j * 5f, 0f, (float)i * 5f), Quaternion.identity);
            }
            yield return null;
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.roamingUnitId != 0)
        {
            List<SpawnNode> nodes = GetSpawnNodes();
            bool valid = false;
            for (int k = 0; k < nodes.Count; k++)
            {
                if (nodes[k].IsOfType(SpawnNodeId.ROAMING))
                {
                    valid = true;
                    break;
                }
            }
            if (valid)
            {
                UnitId unitId = (UnitId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.roamingUnitId;
                UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>((int)unitId);
                int maxRank2 = 0;
                for (int u2 = 0; u2 < PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0].Units.Count; u2++)
                {
                    UnitSave save = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0].Units[u2];
                    maxRank2 = (((int)save.rankId <= maxRank2) ? maxRank2 : ((int)save.rankId));
                }
                UnitRankData rankData2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>(maxRank2);
                int maxRank = 0;
                for (int u = 0; u < PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].Units.Count; u++)
                {
                    UnitSave save2 = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].Units[u];
                    maxRank = (((int)save2.rankId <= maxRank) ? maxRank : ((int)save2.rankId));
                }
                UnitRankData rankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>(maxRank);
                Unit unit = Unit.GenerateUnit(unitId, (int)((float)(rankData2.Rank + rankData.Rank) / 2f));
                if (!string.IsNullOrEmpty(unit.Data.FirstName))
                {
                    unit.UnitSave.stats.name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(unit.Data.FirstName);
                }
                int rating = 0;
                UnitFactory.RaiseAttributes(PandoraSingleton<MissionManager>.Instance.NetworkTyche, PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>(), unit, ref rating, 999999);
                int teamIdx2 = 0;
                for (int t = 0; t < missionSave.teams.Count; t++)
                {
                    teamIdx2 = ((missionSave.teams[t] <= teamIdx2) ? teamIdx2 : missionSave.teams[t]);
                }
                teamIdx2++;
                MissionWarbandSave missionWar2 = new MissionWarbandSave(unitData.WarbandId, CampaignWarbandId.NONE, "roamingWarbandName", string.Empty, "roamingPlayerName", 0, 0, PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0].PlayerIndex, PlayerTypeId.AI, null);
                WarbandController warCtrlr2 = new WarbandController(missionWar2, DeploymentId.ROAMING, PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count, teamIdx2, PrimaryObjectiveTypeId.NONE, 0, 0);
                PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Add(warCtrlr2);
                warCtrlr2.GenerateUnit(unit.UnitSave, defpos, Quaternion.identity);
            }
            else
            {
                PandoraDebug.LogWarning("No roaming point in ground");
            }
        }
        yield return null;
    }

    private void GenerateReinforcements()
    {
        Vector3 position = new Vector3(500f, 150f, 500f);
        for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.reinforcementsIdx.Count; i++)
        {
            int key = PandoraSingleton<MissionStartData>.Instance.reinforcementsIdx[i].Key;
            int value = PandoraSingleton<MissionStartData>.Instance.reinforcementsIdx[i].Value;
            PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[value].GenerateUnit(PandoraSingleton<MissionStartData>.Instance.units[key].unitSave, position, Quaternion.identity);
        }
    }

    private IEnumerator DeployUnits()
    {
        PandoraSingleton<MissionStartData>.instance.spawnZones = new List<SpawnZone>();
        PandoraSingleton<MissionStartData>.instance.spawnZones.AddRange(ground.GetComponentsInChildren<SpawnZone>());
        PandoraSingleton<MissionStartData>.instance.spawnNodes = new List<SpawnNode>[PandoraSingleton<MissionManager>.instance.WarbandCtrlrs.Count];
        spawnNodes = GetSpawnNodes();
        for (int m = 0; m < PandoraSingleton<MissionManager>.instance.WarbandCtrlrs.Count; m++)
        {
            PandoraSingleton<MissionStartData>.instance.spawnNodes[m] = new List<SpawnNode>();
            Deploy(PandoraSingleton<MissionManager>.instance.WarbandCtrlrs[m], m);
            yield return null;
        }
        while (wagonQueued > 0)
        {
            yield return null;
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign)
        {
            int playerWarbandIdx = -1;
            for (int l = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count - 1; l >= 0; l--)
            {
                if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[l].CampaignWarbandId == CampaignWarbandId.NONE)
                {
                    playerWarbandIdx = l;
                    break;
                }
            }
            if (playerWarbandIdx != -1)
            {
                WarbandController playerWarband = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[playerWarbandIdx];
                for (int k = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count - 1; k >= 0; k--)
                {
                    if (k != playerWarbandIdx)
                    {
                        WarbandController otherWarband = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[k];
                        if (otherWarband.playerTypeId == PlayerTypeId.PLAYER && otherWarband.teamIdx == playerWarband.teamIdx)
                        {
                            for (int u = 0; u < otherWarband.unitCtrlrs.Count; u++)
                            {
                                otherWarband.unitCtrlrs[u].unit.warbandIdx = playerWarband.idx;
                                playerWarband.unitCtrlrs.Add(otherWarband.unitCtrlrs[u]);
                                playerWarband.OnUnitCreated(otherWarband.unitCtrlrs[u]);
                            }
                            PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.RemoveAt(k);
                        }
                    }
                }
            }
        }
        PandoraSingleton<MissionManager>.Instance.excludedUnits = new List<UnitController>();
        for (int w = 0; w < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; w++)
        {
            WarbandController warCtrlr = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[w];
            warCtrlr.idx = w;
            for (int i = warCtrlr.unitCtrlrs.Count - 1; i >= 0; i--)
            {
                UnitController ctrlr = warCtrlr.unitCtrlrs[i];
                ctrlr.unit.warbandIdx = w;
            }
            warCtrlr.InitBlackLists();
            if (warCtrlr.SquadManager != null)
            {
                warCtrlr.SquadManager.FormSquads();
            }
            PandoraSingleton<MissionManager>.Instance.allUnitsList.AddRange(warCtrlr.unitCtrlrs);
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
        {
            for (int nodeIndex = 0; nodeIndex < spawnNodes.Count; nodeIndex++)
            {
                SpawnNode node = spawnNodes[nodeIndex];
                if (node != null)
                {
                    UnityEngine.Object.DestroyImmediate(node.gameObject);
                }
            }
            for (int zoneIndex2 = 0; zoneIndex2 < PandoraSingleton<MissionStartData>.Instance.spawnZones.Count; zoneIndex2++)
            {
                SpawnZone zone2 = PandoraSingleton<MissionStartData>.Instance.spawnZones[zoneIndex2];
                if (zone2 != null)
                {
                    UnityEngine.Object.DestroyImmediate(zone2.gameObject);
                }
            }
            PandoraSingleton<MissionStartData>.Instance.spawnNodes = null;
            PandoraSingleton<MissionStartData>.Instance.spawnZones = null;
            yield break;
        }
        for (int zoneIndex = 0; zoneIndex < PandoraSingleton<MissionStartData>.Instance.spawnZones.Count; zoneIndex++)
        {
            SpawnZone zone = PandoraSingleton<MissionStartData>.Instance.spawnZones[zoneIndex];
            if (zone != null)
            {
                Renderer[] r = zone.GetComponentsInChildren<Renderer>();
                for (int j = 0; j < r.Length; j++)
                {
                    r[j].enabled = false;
                }
            }
        }
    }

    public void Deploy(WarbandController warCtrlr, int idx)
    {
        deployedZones = new List<SpawnZone>();
        switch (warCtrlr.deploymentId)
        {
            case DeploymentId.WAGON:
                DeployInZone(warCtrlr, SpawnZoneId.START, SpawnNodeId.START, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.STRIKE_TEAM:
                {
                    List<UnitController> list = new List<UnitController>();
                    List<UnitController> list2 = new List<UnitController>();
                    List<UnitController> list3 = new List<UnitController>();
                    int num = warCtrlr.unitCtrlrs.Count / 3;
                    List<int> list4 = new List<int>(warCtrlr.unitCtrlrs.Count);
                    for (int i = 0; i < warCtrlr.unitCtrlrs.Count; i++)
                    {
                        if (warCtrlr.unitCtrlrs[i].unit.IsImpressive)
                        {
                            list3.Add(warCtrlr.unitCtrlrs[i]);
                        }
                        else
                        {
                            list4.Add(i);
                        }
                    }
                    for (int j = 0; j < num; j++)
                    {
                        int index = networkTyche.Rand(0, list4.Count);
                        list.Add(warCtrlr.unitCtrlrs[list4[index]]);
                        list4.RemoveAt(index);
                    }
                    for (int k = 0; k < num; k++)
                    {
                        int index2 = networkTyche.Rand(0, list4.Count);
                        list2.Add(warCtrlr.unitCtrlrs[list4[index2]]);
                        list4.RemoveAt(index2);
                    }
                    for (int l = 0; l < list4.Count; l++)
                    {
                        list3.Add(warCtrlr.unitCtrlrs[list4[l]]);
                    }
                    DeployInZone(warCtrlr, SpawnZoneId.STRIKE_START, SpawnNodeId.STRIKE, list3, idx);
                    DeployInZone(warCtrlr, SpawnZoneId.STRIKE, SpawnNodeId.STRIKE, list, idx);
                    DeployInZone(warCtrlr, SpawnZoneId.STRIKE, SpawnNodeId.STRIKE, list2, idx);
                    break;
                }
            case DeploymentId.SCATTERED:
                DeployInZone(warCtrlr, SpawnZoneId.SCATTER, 12, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.EXPLORING:
                DeployInZone(warCtrlr, SpawnZoneId.SCATTER, SpawnNodeId.INDOOR, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.SCOUTING:
                DeployInZone(warCtrlr, SpawnZoneId.SCATTER, SpawnNodeId.OUTDOOR, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.AMBUSHED:
                DeployInZone(warCtrlr, SpawnZoneId.AMBUSH, SpawnNodeId.INDOOR, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.AMBUSHER:
                DeployInZone(warCtrlr, SpawnZoneId.AMBUSH, SpawnNodeId.OUTDOOR, warCtrlr.unitCtrlrs, idx);
                break;
            case DeploymentId.CAMPAIGN_PLAYER:
            case DeploymentId.CAMPAIGN_AI:
                DeployCampaign(warCtrlr, warCtrlr.unitCtrlrs);
                break;
            case DeploymentId.ROAMING:
                DeployRoaming(warCtrlr, idx);
                break;
        }
        deployedZones.Clear();
        deployedZones = null;
    }

    private void DeployRoaming(WarbandController warCtrlr, int idx)
    {
        List<SpawnNode> list = new List<SpawnNode>();
        for (int i = 0; i < spawnNodes.Count; i++)
        {
            if (!spawnNodes[i].claimed && spawnNodes[i].IsOfType(SpawnNodeId.ROAMING))
            {
                list.Add(spawnNodes[i]);
                PandoraSingleton<MissionStartData>.Instance.spawnNodes[idx].Add(spawnNodes[i]);
            }
        }
        SpawnNode spawnNode = list[PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count)];
        spawnNode.claimed = true;
        Debug.Log("DEPLOYING UNIT ON NODE AT POSITION : " + spawnNode.transform.position);
        warCtrlr.unitCtrlrs[0].transform.rotation = spawnNode.transform.rotation;
        warCtrlr.unitCtrlrs[0].SetFixed(spawnNode.transform.position, fix: true);
    }

    private void DeployCampaign(WarbandController warCtrlr, List<UnitController> units)
    {
        CampaignMissionJoinCampaignWarbandData missionWarData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionJoinCampaignWarbandData>(new string[2]
        {
            "fk_campaign_mission_id",
            "fk_campaign_warband_id"
        }, new string[2]
        {
            PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId.ToString(),
            ((int)warCtrlr.CampaignWarbandId).ToString()
        })[0];
        warCtrlr.canRout = missionWarData.CanRout;
        PandoraDebug.LogInfo("Looking for zone : " + missionWarData.DeployZone);
        SpawnZone spawnZone = PandoraSingleton<MissionStartData>.Instance.spawnZones.Find((SpawnZone x) => x.name == missionWarData.DeployZone);
        spawnZone.Claim(DeploymentId.CAMPAIGN_PLAYER);
        List<SpawnNode> list = new List<SpawnNode>();
        list.AddRange(spawnZone.GetComponentsInChildren<SpawnNode>());
        List<SpawnNode> list2 = new List<SpawnNode>();
        List<SpawnNode> list3 = new List<SpawnNode>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsOfType(SpawnNodeId.WAGON) && !list[i].claimed)
            {
                list2.Add(list[i]);
                if (list[i].IsOfType(SpawnNodeId.IMPRESSIVE))
                {
                    list3.Add(list[i]);
                }
            }
        }
        bool flag = false;
        if (warCtrlr.NeedWagon)
        {
            for (int j = 0; j < list.Count; j++)
            {
                SpawnNode spawnNode = list[j];
                if (spawnNode.IsOfType(SpawnNodeId.WAGON) && !spawnNode.claimed)
                {
                    spawnNode.claimed = true;
                    SpawnWagon(spawnNode, warCtrlr);
                    flag = true;
                    break;
                }
            }
        }
        for (int num = units.Count - 1; num >= 0; num--)
        {
            if (units[num].unit.IsImpressive)
            {
                SpawnNode spawnNode2 = null;
                if (missionWarData.CampaignWarbandId == CampaignWarbandId.NONE)
                {
                    int index = networkTyche.Rand(0, list3.Count);
                    spawnNode2 = list3[index];
                    spawnNode2.claimed = true;
                    list3.RemoveAt(index);
                    list2.Remove(spawnNode2);
                }
                else
                {
                    CampaignWarbandJoinCampaignUnitData campUnitData2 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandJoinCampaignUnitData>(new string[2]
                    {
                        "fk_campaign_warband_id",
                        "fk_campaign_unit_id"
                    }, new string[2]
                    {
                        ((int)warCtrlr.CampaignWarbandId).ToString(),
                        units[num].unit.UnitSave.campaignId.ToString()
                    })[0];
                    spawnNode2 = list.Find((SpawnNode x) => x.name == campUnitData2.DeployNode);
                    spawnNode2.claimed = true;
                }
                units[num].transform.rotation = spawnNode2.transform.rotation;
                units[num].SetFixed(spawnNode2.transform.position, fix: true);
            }
        }
        for (int k = 0; k < units.Count; k++)
        {
            if (units[k].unit.IsImpressive || units[k].unit.UnitSave.isReinforcement)
            {
                continue;
            }
            SpawnNode spawnNode3 = null;
            if (missionWarData.CampaignWarbandId == CampaignWarbandId.NONE)
            {
                int index2 = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list2.Count);
                spawnNode3 = list2[index2];
                spawnNode3.claimed = true;
                list2.RemoveAt(index2);
            }
            else
            {
                if (units[k].unit.Id == UnitId.BLUE_HORROR)
                {
                    continue;
                }
                CampaignWarbandJoinCampaignUnitData campUnitData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandJoinCampaignUnitData>(new string[2]
                {
                    "fk_campaign_warband_id",
                    "fk_campaign_unit_id"
                }, new string[2]
                {
                    ((int)warCtrlr.CampaignWarbandId).ToString(),
                    units[k].unit.UnitSave.campaignId.ToString()
                })[0];
                spawnNode3 = list.Find((SpawnNode x) => x.name == campUnitData.DeployNode);
                spawnNode3.claimed = true;
            }
            units[k].transform.rotation = spawnNode3.transform.rotation;
            units[k].SetFixed(spawnNode3.transform.position, fix: true);
        }
    }

    private void DeployInZone(WarbandController warCtrlr, SpawnZoneId id, SpawnNodeId nodeId, List<UnitController> units, int idx)
    {
        DeployInZone(warCtrlr, id, 1 << (int)(nodeId - 1), units, idx);
    }

    private void DeployInZone(WarbandController warCtrlr, SpawnZoneId zoneId, int nodeTypes, List<UnitController> units, int idx)
    {
        List<SpawnZone> list = new List<SpawnZone>();
        if (zoneId != SpawnZoneId.STRIKE)
        {
            for (int i = 0; i < PandoraSingleton<MissionStartData>.instance.spawnZones.Count; i++)
            {
                SpawnZone spawnZone = PandoraSingleton<MissionStartData>.instance.spawnZones[i];
                if (spawnZone.type == zoneId && !spawnZone.IsClaimed(warCtrlr.deploymentId))
                {
                    list.Add(spawnZone);
                }
            }
        }
        else
        {
            list.AddRange(deployedZones[0].GetComponentsInChildren<SpawnZone>());
            for (int num = list.Count - 1; num >= 0; num--)
            {
                if (list[num].IsClaimed(warCtrlr.deploymentId))
                {
                    list.RemoveAt(num);
                }
            }
        }
        int index = networkTyche.Rand(0, list.Count);
        SpawnZone spawnZone2 = list[index];
        spawnZone2.Claim(warCtrlr.deploymentId);
        deployedZones.Add(spawnZone2);
        List<SpawnNode> list2 = new List<SpawnNode>();
        List<SpawnNode> list3 = new List<SpawnNode>();
        List<SpawnNode> list4 = new List<SpawnNode>();
        bool flag = !string.IsNullOrEmpty(warCtrlr.WarData.Wagon);
        for (int j = 0; j < spawnNodes.Count; j++)
        {
            SpawnNode spawnNode = spawnNodes[j];
            if (!spawnNode.claimed && (spawnNode.types & nodeTypes) != 0 && !spawnNode.IsOfType(SpawnNodeId.WAGON) && ((zoneId == SpawnZoneId.SCATTER && Vector3.SqrMagnitude(spawnZone2.transform.position - spawnNode.transform.position) < spawnZone2.range * spawnZone2.range && !InsideZone(list, spawnNode.transform.position)) || (zoneId != SpawnZoneId.SCATTER && spawnZone2.bounds.Contains(spawnNode.transform.position))))
            {
                list3.Add(spawnNode);
                PandoraSingleton<MissionStartData>.Instance.spawnNodes[idx].Add(spawnNode);
                if (spawnNode.IsOfType(SpawnNodeId.IMPRESSIVE))
                {
                    list4.Add(spawnNode);
                }
            }
            else if (flag && warCtrlr.deploymentId == DeploymentId.AMBUSHED && !spawnNode.claimed && spawnNode.IsOfType(SpawnNodeId.WAGON) && spawnNode.IsOfType(SpawnNodeId.INDOOR) && spawnZone2.bounds.Contains(spawnNode.transform.position))
            {
                list2.Add(spawnNode);
            }
        }
        if (flag && spawnZone2.type != SpawnZoneId.STRIKE)
        {
            if (warCtrlr.deploymentId != DeploymentId.AMBUSHED)
            {
                SpawnNode[] componentsInChildren = spawnZone2.GetComponentsInChildren<SpawnNode>();
                foreach (SpawnNode spawnNode2 in componentsInChildren)
                {
                    if (spawnNode2.IsOfType(SpawnNodeId.WAGON))
                    {
                        list2.Add(spawnNode2);
                    }
                }
            }
            int index2 = networkTyche.Rand(0, list2.Count);
            SpawnNode spawnNode3 = list2[index2];
            spawnNode3.claimed = true;
            SpawnWagon(spawnNode3, warCtrlr);
        }
        if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
        {
            return;
        }
        for (int num2 = units.Count - 1; num2 >= 0; num2--)
        {
            if (units[num2].unit.IsImpressive)
            {
                int index3 = networkTyche.Rand(0, list4.Count);
                SpawnNode spawnNode4 = list4[index3];
                spawnNode4.claimed = true;
                list4.RemoveAt(index3);
                list3.Remove(spawnNode4);
                Debug.Log("DEPLOYING UNIT ON NODE AT POSITION : " + spawnNode4.transform.position);
                units[num2].transform.rotation = spawnNode4.transform.rotation;
                units[num2].SetFixed(spawnNode4.transform.position, fix: true);
            }
        }
        for (int l = 0; l < units.Count; l++)
        {
            if (!units[l].unit.IsImpressive)
            {
                int index4 = networkTyche.Rand(0, list3.Count);
                SpawnNode spawnNode5 = list3[index4];
                spawnNode5.claimed = true;
                Debug.Log("DEPLOYING UNIT ON NODE AT POSITION : " + spawnNode5.transform.position);
                units[l].transform.rotation = spawnNode5.transform.rotation;
                units[l].SetFixed(spawnNode5.transform.position, fix: true);
                list3.RemoveAt(index4);
            }
        }
    }

    private bool InsideZone(List<SpawnZone> zones, Vector3 pos)
    {
        for (int i = 0; i < zones.Count; i++)
        {
            SpawnZone spawnZone = zones[i];
            if (spawnZone.bounds.Contains(pos))
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnWagon(SpawnNode wagonNode, WarbandController warCtrlr)
    {
        bool flag = false;
        flag = ((wagonNode.overrideStyle != SpawnNodeId.INDOOR && wagonNode.overrideStyle != SpawnNodeId.OUTDOOR) ? wagonNode.IsOfType(SpawnNodeId.INDOOR) : (wagonNode.overrideStyle == SpawnNodeId.INDOOR));
        Vector3 position = wagonNode.transform.position;
        Quaternion rotation = wagonNode.transform.rotation;
        wagonQueued++;
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/" + ((!flag) ? "wagons/" : "chests/"), AssetBundleId.PROPS, ((!flag) ? warCtrlr.WarData.Wagon : warCtrlr.WarData.Chest) + ".prefab", delegate (UnityEngine.Object wagPrefab)
        {
            wagonQueued--;
            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(wagPrefab);
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            warCtrlr.SetWagon(gameObject);
            if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign && warCtrlr.playerTypeId == PlayerTypeId.AI)
            {
                DataFactory instance = PandoraSingleton<DataFactory>.Instance;
                string[] fields = new string[2]
                {
                    "warband_rank",
                    "fk_search_density_id"
                };
                string[] obj = new string[2]
                {
                    lowestWarbandRank.ToString(),
                    null
                };
                int searchDensityId = missionSave.searchDensityId;
                obj[1] = searchDensityId.ToString();
                List<SearchDensityLootData> datas = instance.InitData<SearchDensityLootData>(fields, obj);
                SearchDensityLootData randomRatio = SearchDensityLootData.GetRandomRatio(datas, networkTyche);
                int itemCount = networkTyche.Rand(randomRatio.ItemMin, randomRatio.ItemMax + 1);
                FillSearchPoint(warCtrlr.wagon.chest, randomRatio, lowestWarbandRank, itemCount);
            }
        });
    }

    private TrapId GetRandomTrapId(TrapTypeId trapTypeId)
    {
        List<TrapTypeJoinTrapData> list = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeJoinTrapData>("fk_trap_type_id", trapTypeId.ToIntString());
        return list[networkTyche.Rand(0, list.Count)].TrapId;
    }

    private void GenerateTrapsAsync()
    {
        traps = new List<Trap>();
        trapData = new List<KeyValuePair<TrapTypeData, bool>>();
        trapNodes = new List<TrapNode>();
        traps.AddRange(ground.GetComponentsInChildren<Trap>());
        for (int i = 0; i < traps.Count; i++)
        {
            Trap trap = traps[i];
            TrapTypeData data = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeData>((int)trap.defaultType);
            trap.Init(data, PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
            PandoraSingleton<MissionManager>.Instance.triggerPoints.Add(trap);
        }
        if (trapCount == 0)
        {
            return;
        }
        trapNodes.AddRange(ground.GetComponentsInChildren<TrapNode>());
        if (trapNodes.Count == 0)
        {
            return;
        }
        Dictionary<TrapTypeId, List<TrapNode>> dictionary = new Dictionary<TrapTypeId, List<TrapNode>>();
        for (int j = 0; j < trapNodes.Count; j++)
        {
            TrapNode trapNode = trapNodes[j];
            if (!dictionary.ContainsKey(trapNode.typeId))
            {
                dictionary[trapNode.typeId] = new List<TrapNode>();
            }
            dictionary[trapNode.typeId].Add(trapNode);
        }
        foreach (TrapTypeId key in dictionary.Keys)
        {
            TrapTypeData trapTypeData = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeData>((int)key);
            int num = (int)Mathf.Ceil((float)(dictionary[key].Count * trapTypeData.Perc) / 100f);
            for (int k = 0; k < num; k++)
            {
                int index = networkTyche.Rand(0, dictionary[key].Count);
                TrapNode trapNode2 = dictionary[key][index];
                TrapId trapId = TrapId.NONE;
                TrapZone component = trapNode2.transform.parent.gameObject.GetComponent<TrapZone>();
                if (component != null && component.currentTrapId != 0)
                {
                    trapId = component.currentTrapId;
                }
                else
                {
                    trapId = GetRandomTrapId(trapTypeData.Id);
                    if (component != null)
                    {
                        component.currentTrapId = trapId;
                    }
                }
                string name = PandoraSingleton<DataFactory>.Instance.InitData<TrapData>((int)trapId).Name;
                AddSceneJob(name, trapNode2.transform);
                trapData.Add(new KeyValuePair<TrapTypeData, bool>(trapTypeData, trapNode2.forceInactive));
                dictionary[key].RemoveAt(index);
            }
        }
    }

    private void TrapsPostProcess()
    {
        List<Trap> availableTraps = GetAvailableTraps();
        int num = Mathf.Min(trapCount, availableTraps.Count);
        for (int i = 0; i < num; i++)
        {
            if (availableTraps.Count <= 0)
            {
                break;
            }
            int index = networkTyche.Rand(0, availableTraps.Count);
            Trap trap = availableTraps[index];
            PandoraSingleton<MissionManager>.Instance.triggerPoints.Add(trap);
            traps.Remove(trap);
            availableTraps.RemoveAt(index);
            float @float = Constant.GetFloat(ConstantId.MIN_TRAP_DISTANCE);
            @float *= @float;
            for (int num2 = availableTraps.Count - 1; num2 >= 0; num2--)
            {
                if (Vector3.SqrMagnitude(availableTraps[num2].transform.position - trap.transform.position) < @float)
                {
                    availableTraps.RemoveAt(num2);
                }
            }
        }
        for (int num3 = traps.Count - 1; num3 >= 0; num3--)
        {
            UnityEngine.Object.Destroy(traps[num3]);
        }
        for (int j = 0; j < trapNodes.Count; j++)
        {
            TrapNode trapNode = trapNodes[j];
            UnityEngine.Object.Destroy(trapNode.gameObject);
        }
        trapNodes.Clear();
    }

    private List<Trap> GetAvailableTraps()
    {
        List<SpawnNode> list = GetSpawnNodes();
        List<Trap> list2 = new List<Trap>();
        for (int i = 0; i < traps.Count; i++)
        {
            Trap trap = traps[i];
            bool flag = !trap.forceInactive;
            if (flag)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (!flag)
                    {
                        break;
                    }
                    flag &= (Vector3.SqrMagnitude(trap.trigger.transform.position - list[j].transform.position) > 25f);
                }
            }
            if (flag)
            {
                list2.Add(trap);
            }
        }
        return list2;
    }

    private IEnumerator ReloadTraps()
    {
        loadingTraps = 0;
        List<uint> usedTraps = PandoraSingleton<MissionStartData>.Instance.usedTraps;
        for (int usedTrapIdx = 0; usedTrapIdx < usedTraps.Count; usedTrapIdx++)
        {
            List<TriggerPoint> missionTraps = PandoraSingleton<MissionManager>.Instance.triggerPoints;
            for (int generatedTrapsIdx = 0; generatedTrapsIdx < missionTraps.Count; generatedTrapsIdx++)
            {
                if (missionTraps[generatedTrapsIdx].guid == usedTraps[usedTrapIdx])
                {
                    UnityEngine.Object.Destroy(missionTraps[generatedTrapsIdx]);
                    break;
                }
            }
        }
        List<EndDynamicTrap> dynamicTraps = PandoraSingleton<MissionStartData>.Instance.dynamicTraps;
        for (int i = 0; i < dynamicTraps.Count; i++)
        {
            EndDynamicTrap endDynamicTrap = dynamicTraps[i];
            if (!endDynamicTrap.consumed)
            {
                loadingTraps++;
                EndDynamicTrap endDynamicTrap2 = dynamicTraps[i];
                int trapTypeId = endDynamicTrap2.trapTypeId;
                EndDynamicTrap endDynamicTrap3 = dynamicTraps[i];
                int teamIdx = endDynamicTrap3.teamIdx;
                EndDynamicTrap endDynamicTrap4 = dynamicTraps[i];
                Vector3 pos = endDynamicTrap4.pos;
                EndDynamicTrap endDynamicTrap5 = dynamicTraps[i];
                Trap.SpawnTrap((TrapTypeId)trapTypeId, teamIdx, pos, endDynamicTrap5.rot, delegate
                {
                    loadingTraps--;
                }, unload: false);
            }
        }
        while (loadingTraps > 0)
        {
            yield return null;
        }
        PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
    }

    private void ReloadDestructibles()
    {
        List<EndDestructible> destructibles = PandoraSingleton<MissionStartData>.Instance.destructibles;
        for (int i = 0; i < destructibles.Count; i++)
        {
            EndDestructible endDestructible = destructibles[i];
            if (endDestructible.onwerGuid == 0)
            {
                bool flag = false;
                for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.triggerPoints.Count; j++)
                {
                    if (PandoraSingleton<MissionManager>.Instance.triggerPoints[j].guid == endDestructible.guid)
                    {
                        flag = true;
                        Destructible destructible = (Destructible)PandoraSingleton<MissionManager>.Instance.triggerPoints[j];
                        destructible.CurrentWounds = endDestructible.wounds;
                        if (endDestructible.wounds <= 0)
                        {
                            destructible.Deactivate();
                        }
                        endDestructible.guid = destructible.guid;
                    }
                }
            }
            else if (endDestructible.wounds > 0)
            {
                Destructible.Spawn(endDestructible.destructibleId, PandoraSingleton<MissionManager>.Instance.GetUnitController(endDestructible.onwerGuid), endDestructible.position, endDestructible.wounds);
            }
        }
    }

    private void GenerateWyrdStonesAsync()
    {
        wyrdTypeData = new List<WyrdstoneTypeData>();
        SearchPoint[] componentsInChildren = ground.GetComponentsInChildren<SearchPoint>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        }
        ActivatePoint[] componentsInChildren2 = ground.GetComponentsInChildren<ActivatePoint>();
        for (int j = 0; j < componentsInChildren2.Length; j++)
        {
            componentsInChildren2[j].Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        }
        List<SearchArea> list = new List<SearchArea>();
        list.AddRange(ground.GetComponentsInChildren<SearchArea>());
        List<SearchZone> list2 = new List<SearchZone>();
        list2.AddRange(ground.GetComponentsInChildren<SearchZone>());
        searchZones = new List<SearchZone>();
        for (int k = 0; k < list2.Count; k++)
        {
            SearchZone searchZone = list2[k];
            if (InsideArea(list, searchZone.transform.position))
            {
                searchZones.Add(searchZone);
            }
        }
        allSearchNodes = new List<SearchNode>();
        allSearchNodes.AddRange(ground.GetComponentsInChildren<SearchNode>());
        searchNodes = new List<SearchNode>();
        for (int l = 0; l < allSearchNodes.Count; l++)
        {
            SearchNode searchNode = allSearchNodes[l];
            if (InsideArea(list, searchNode.transform.position))
            {
                searchNodes.Add(searchNode);
            }
        }
        if (missionSave.wyrdPlacementId != 0 && missionSave.wyrdDensityId != 0)
        {
            GenerateWyrdStones((WyrdstonePlacementId)missionSave.wyrdPlacementId, (WyrdstoneDensityId)missionSave.wyrdDensityId);
        }
    }

    private void GenerateSearchAsync()
    {
        if (missionSave.searchDensityId != 0)
        {
            GenerateSearch((SearchDensityId)missionSave.searchDensityId);
        }
    }

    private void SearchPostProcess()
    {
        ZoneAoe[] componentsInChildren = ground.GetComponentsInChildren<ZoneAoe>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].AutoInit();
        }
        Destructible[] componentsInChildren2 = ground.GetComponentsInChildren<Destructible>();
        for (int j = 0; j < componentsInChildren2.Length; j++)
        {
            componentsInChildren2[j].AutoInit();
        }
        List<SearchArea> list = new List<SearchArea>();
        list.AddRange(ground.GetComponentsInChildren<SearchArea>());
        List<SearchZone> list2 = new List<SearchZone>();
        list2.AddRange(ground.GetComponentsInChildren<SearchZone>());
        for (int k = 0; k < list.Count; k++)
        {
            SearchArea searchArea = list[k];
            UnityEngine.Object.Destroy(searchArea.gameObject);
        }
        for (int l = 0; l < list2.Count; l++)
        {
            SearchZone searchZone = list2[l];
            UnityEngine.Object.Destroy(searchZone.gameObject);
        }
        for (int m = 0; m < allSearchNodes.Count; m++)
        {
            SearchNode searchNode = allSearchNodes[m];
            UnityEngine.Object.Destroy(searchNode.gameObject);
        }
        PandoraSingleton<MissionManager>.Instance.numWyrdstones = PandoraSingleton<MissionManager>.Instance.GetInitialWyrdstoneCount();
    }

    private void GenerateSearchPoints()
    {
        SearchPoint[] componentsInChildren = ground.GetComponentsInChildren<SearchPoint>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        }
        ActivatePoint[] componentsInChildren2 = ground.GetComponentsInChildren<ActivatePoint>();
        for (int j = 0; j < componentsInChildren2.Length; j++)
        {
            componentsInChildren2[j].Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        }
        List<SearchArea> list = new List<SearchArea>();
        list.AddRange(ground.GetComponentsInChildren<SearchArea>());
        List<SearchZone> list2 = new List<SearchZone>();
        list2.AddRange(ground.GetComponentsInChildren<SearchZone>());
        searchZones = new List<SearchZone>();
        for (int k = 0; k < list2.Count; k++)
        {
            SearchZone searchZone = list2[k];
            if (InsideArea(list, searchZone.transform.position))
            {
                searchZones.Add(searchZone);
            }
        }
        allSearchNodes = new List<SearchNode>();
        allSearchNodes.AddRange(ground.GetComponentsInChildren<SearchNode>());
        searchNodes = new List<SearchNode>();
        for (int l = 0; l < allSearchNodes.Count; l++)
        {
            SearchNode searchNode = allSearchNodes[l];
            if (InsideArea(list, searchNode.transform.position))
            {
                searchNodes.Add(searchNode);
            }
        }
        if (missionSave.wyrdPlacementId != 0 && missionSave.wyrdDensityId != 0)
        {
            GenerateWyrdStones((WyrdstonePlacementId)missionSave.wyrdPlacementId, (WyrdstoneDensityId)missionSave.wyrdDensityId);
        }
        if (missionSave.searchDensityId != 0)
        {
            GenerateSearch((SearchDensityId)missionSave.searchDensityId);
        }
        ZoneAoe[] componentsInChildren3 = ground.GetComponentsInChildren<ZoneAoe>();
        for (int m = 0; m < componentsInChildren3.Length; m++)
        {
            componentsInChildren3[m].AutoInit();
        }
        Destructible[] componentsInChildren4 = ground.GetComponentsInChildren<Destructible>();
        for (int n = 0; n < componentsInChildren4.Length; n++)
        {
            componentsInChildren4[n].AutoInit();
        }
        for (int num = 0; num < list.Count; num++)
        {
            SearchArea searchArea = list[num];
            UnityEngine.Object.Destroy(searchArea.gameObject);
        }
        for (int num2 = 0; num2 < list2.Count; num2++)
        {
            SearchZone searchZone2 = list2[num2];
            UnityEngine.Object.Destroy(searchZone2.gameObject);
        }
        for (int num3 = 0; num3 < allSearchNodes.Count; num3++)
        {
            SearchNode searchNode2 = allSearchNodes[num3];
            UnityEngine.Object.Destroy(searchNode2.gameObject);
        }
        PandoraSingleton<MissionManager>.Instance.numWyrdstones = PandoraSingleton<MissionManager>.Instance.GetInitialWyrdstoneCount();
    }

    private bool InsideArea(List<SearchArea> areas, Vector3 position)
    {
        for (int i = 0; i < areas.Count; i++)
        {
            SearchArea searchArea = areas[i];
            if (searchArea.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    private Transform GetValidParent(GameObject node)
    {
        GameObject gameObject = node.transform.parent.gameObject;
        if (gameObject.GetComponent<SearchArea>() != null || gameObject.GetComponent<SearchZone>() != null)
        {
            return GetValidParent(gameObject);
        }
        return gameObject.transform;
    }

    private void GenerateWyrdStones(WyrdstonePlacementId placementId, WyrdstoneDensityId densityId)
    {
        WyrdstoneDensityData wyrdstoneDensityData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneDensityData>((int)densityId);
        int num = networkTyche.Rand(wyrdstoneDensityData.SpawnMin, wyrdstoneDensityData.SpawnMax + 1);
        WyrdstoneDensityProgressionData wyrdstoneDensityProgressionData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneDensityProgressionData>(new string[2]
        {
            "fk_wyrdstone_density_id",
            "warband_rank"
        }, new string[2]
        {
            ((int)wyrdstoneDensityData.Id).ToString(),
            lowestWarbandRank.ToString()
        })[0];
        int num2 = (int)Mathf.Ceil((float)(num * wyrdstoneDensityProgressionData.Cluster) / 100f);
        int num3 = (int)Mathf.Ceil((float)(num * wyrdstoneDensityProgressionData.Shard) / 100f);
        int count = num - num2 - num3;
        List<WyrdstoneTypeId> types = new List<WyrdstoneTypeId>();
        WyrdstoneAddType(types, num2, WyrdstoneTypeId.CLUSTER);
        WyrdstoneAddType(types, num3, WyrdstoneTypeId.SHARD);
        WyrdstoneAddType(types, count, WyrdstoneTypeId.FRAGMENT);
        switch (placementId)
        {
            case (WyrdstonePlacementId)2:
            case (WyrdstonePlacementId)5:
                break;
            case WyrdstonePlacementId.CONCENTRATION:
                DeployWyrdStoneZone(SearchZoneId.WYRDSTONE_CONCENTRATION, num, types);
                break;
            case WyrdstonePlacementId.CLUMP:
                while (num > 0)
                {
                    int num5 = Mathf.Min(networkTyche.Rand(3, 7), num);
                    num -= num5;
                    DeployWyrdStoneZone(SearchZoneId.WYRDSTONE_CLUSTER, num5, types);
                }
                break;
            case WyrdstonePlacementId.CLUMP_AND_SPREAD:
                {
                    int num4 = networkTyche.Rand(3, 7);
                    DeployWyrdStoneZone(SearchZoneId.WYRDSTONE_CLUSTER, num4, types);
                    num -= num4;
                    DeployWyrdStoneZone(SearchZoneId.NONE, num, types);
                    break;
                }
            case WyrdstonePlacementId.SPREAD:
                DeployWyrdStoneZone(SearchZoneId.NONE, num, types);
                break;
        }
    }

    private void WyrdstoneAddType(List<WyrdstoneTypeId> types, int count, WyrdstoneTypeId id)
    {
        for (int i = 0; i < count; i++)
        {
            types.Add(id);
        }
    }

    private void DeployWyrdStoneZone(SearchZoneId zoneId, int count, List<WyrdstoneTypeId> types)
    {
        List<SearchNode> list = new List<SearchNode>();
        switch (zoneId)
        {
            case SearchZoneId.WYRDSTONE_CONCENTRATION:
            case SearchZoneId.WYRDSTONE_CLUSTER:
                {
                    List<SearchZone> list2 = new List<SearchZone>();
                    for (int num = searchZones.Count - 1; num >= 0; num--)
                    {
                        if (!searchZones[num].claimed && searchZones[num].type == zoneId)
                        {
                            list2.Add(searchZones[num]);
                        }
                    }
                    if (list2.Count == 0)
                    {
                        PandoraDebug.LogWarning("No WyrdStone Zone of type " + zoneId + " found. Either none on the map or to close to units", "SEARCH_POINTS", this);
                        return;
                    }
                    int index = networkTyche.Rand(0, list2.Count);
                    SearchZone searchZone = list2[index];
                    searchZone.claimed = true;
                    for (int j = 0; j < searchNodes.Count; j++)
                    {
                        SearchNode searchNode2 = searchNodes[j];
                        if (!searchNode2.claimed && searchNode2.IsOfType(SearchNodeId.WYRDSTONE) && searchZone.Contains(searchNode2.transform.position))
                        {
                            list.Add(searchNode2);
                            searchNode2.claimed = true;
                        }
                    }
                    break;
                }
            case SearchZoneId.NONE:
                {
                    for (int i = 0; i < searchNodes.Count; i++)
                    {
                        SearchNode searchNode = searchNodes[i];
                        if (!searchNode.claimed && searchNode.IsOfType(SearchNodeId.WYRDSTONE))
                        {
                            list.Add(searchNode);
                        }
                    }
                    break;
                }
        }
        while (count > 0 && list.Count > 0)
        {
            int index2 = networkTyche.Rand(0, list.Count);
            int index3 = networkTyche.Rand(0, types.Count);
            SpawnWyrdStone(types[index3], list[index2]);
            list.RemoveAt(index2);
            types.RemoveAt(index3);
            count--;
        }
    }

    private void SpawnWyrdStone(WyrdstoneTypeId typeId, SearchNode node)
    {
        node.claimed = true;
        WyrdstoneTypeData wyrdstoneTypeData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneTypeData>((int)typeId);
        List<string> list = new List<string>();
        List<WyrdstoneTypeJoinWyrdstoneData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneTypeJoinWyrdstoneData>("fk_wyrdstone_type_id", wyrdstoneTypeData.Id.ToIntString());
        for (int i = 0; i < list2.Count; i++)
        {
            int wyrdstoneId = (int)list2[i].WyrdstoneId;
            WyrdstoneData wyrdstoneData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneData>(wyrdstoneId);
            if ((wyrdstoneData.Outdoor && node.IsOfType(SearchNodeId.OUTDOOR)) || (!wyrdstoneData.Outdoor && node.IsOfType(SearchNodeId.INDOOR)))
            {
                list.Add(wyrdstoneData.Name);
            }
        }
        int index = networkTyche.Rand(0, list.Count);
        string assetName = list[index];
        AddSceneJob(assetName, GetValidParent(node.gameObject), node.transform);
        wyrdTypeData.Add(wyrdstoneTypeData);
    }

    private void GenerateSearch(SearchDensityId densityId)
    {
        List<SearchData> list = PandoraSingleton<DataFactory>.Instance.InitData<SearchData>();
        List<SearchData> list2 = new List<SearchData>();
        List<SearchData> list3 = new List<SearchData>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Outdoor)
            {
                list2.Add(list[i]);
            }
            else
            {
                list3.Add(list[i]);
            }
        }
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        string[] fields = new string[2]
        {
            "warband_rank",
            "fk_search_density_id"
        };
        string[] obj = new string[2]
        {
            lowestWarbandRank.ToString(),
            null
        };
        int num = (int)densityId;
        obj[1] = num.ToString();
        rewards = instance.InitData<SearchDensityLootData>(fields, obj);
        int num2 = 0;
        for (int num3 = allSearchNodes.Count - 1; num3 >= 0; num3--)
        {
            SearchNode searchNode = allSearchNodes[num3];
            if (!searchNode.claimed && searchNode.IsOfType(SearchNodeId.SEARCH) && searchNode.forceInit)
            {
                num2++;
                SpawnSearch(searchNode, (!searchNode.IsOfType(SearchNodeId.OUTDOOR)) ? list3 : list2, densityId, lowestWarbandRank);
                searchNode.claimed = true;
            }
        }
        List<SearchNode> list4 = new List<SearchNode>();
        for (int j = 0; j < searchNodes.Count; j++)
        {
            SearchNode searchNode2 = searchNodes[j];
            if (!searchNode2.claimed && searchNode2.IsOfType(SearchNodeId.SEARCH))
            {
                list4.Add(searchNode2);
                searchNode2.claimed = true;
            }
        }
        SearchDensityData searchDensityData = PandoraSingleton<DataFactory>.Instance.InitData<SearchDensityData>((int)densityId);
        int num4 = networkTyche.Rand(searchDensityData.SpawnMin, searchDensityData.SpawnMax + 1);
        num4 -= num2;
        num4 = Mathf.Clamp(num4, 0, list4.Count);
        for (int k = 0; k < num4; k++)
        {
            int index = networkTyche.Rand(0, list4.Count);
            SpawnSearch(list4[index], (!list4[index].IsOfType(SearchNodeId.OUTDOOR)) ? list3 : list2, densityId, lowestWarbandRank);
            list4.RemoveAt(index);
        }
    }

    private void SpawnSearch(SearchNode node, List<SearchData> visuals, SearchDensityId densityId, int lowestWarbandRank)
    {
        int index = networkTyche.Rand(0, visuals.Count);
        string name = visuals[index].Name;
        AddSceneJob(name, GetValidParent(node.gameObject), node.transform);
    }

    private void FillSearchPoint(SearchPoint search, SearchDensityLootData densityLootData, int lowestWarbandRank, int itemCount)
    {
        switch (densityLootData.SearchRewardId)
        {
            case SearchRewardId.NOTHING:
            case SearchRewardId.GOLD_LUMPS:
                AddGoldToSearch(search, densityLootData.SearchRewardId, lowestWarbandRank, itemCount);
                break;
            case SearchRewardId.NORMAL_ITEMS:
            case SearchRewardId.GOOD_ITEMS:
            case SearchRewardId.BEST_ITEMS:
            case SearchRewardId.GOOD_ENCHANTED:
            case SearchRewardId.BEST_ENCHANTED_NORMAL:
            case SearchRewardId.BEST_ENCHANTED_MASTER:
                {
                    SearchRewardData searchRewardData = PandoraSingleton<DataFactory>.Instance.InitData<SearchRewardData>((int)densityLootData.SearchRewardId);
                    List<SearchRewardItemData> rewardItems = PandoraSingleton<DataFactory>.Instance.InitData<SearchRewardItemData>(new string[2]
                    {
                "fk_search_reward_id",
                "warband_rank"
                    }, new string[2]
                    {
                ((int)searchRewardData.Id).ToString(),
                lowestWarbandRank.ToString()
                    });
                    for (int i = 0; i < itemCount; i++)
                    {
                        Item itemReward = Item.GetItemReward(rewardItems, networkTyche);
                        search.AddItem(itemReward);
                    }
                    break;
                }
        }
        Item.SortEmptyItems(search.items, 0);
    }

    private void AddGoldToSearch(SearchPoint search, SearchRewardId rewardId, int warbandRank, int itemCount)
    {
        int num = 0;
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        string[] fields = new string[2]
        {
            "fk_search_reward_id",
            "warband_rank"
        };
        string[] array = new string[2];
        int num2 = (int)rewardId;
        array[0] = num2.ToString();
        array[1] = warbandRank.ToString();
        SearchRewardGoldData searchRewardGoldData = instance.InitData<SearchRewardGoldData>(fields, array)[0];
        for (int i = 0; i < itemCount; i++)
        {
            num += networkTyche.Rand(searchRewardGoldData.Min, searchRewardGoldData.Max + 1);
        }
        Item item = new Item(ItemId.GOLD);
        item.Save.amount = num;
        search.AddItem(item);
    }

    private void SetupObjectives()
    {
        List<LocateZone> locateZones = PandoraSingleton<MissionManager>.Instance.GetLocateZones();
        for (int i = 0; i < locateZones.Count; i++)
        {
            locateZones[i].guid = PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID();
        }
        CampaignMissionId campaignId = (CampaignMissionId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId;
        if (campaignId != 0)
        {
            DataFactory instance = PandoraSingleton<DataFactory>.Instance;
            int num = (int)campaignId;
            List<CampaignMissionJoinCampaignWarbandData> list = instance.InitData<CampaignMissionJoinCampaignWarbandData>("fk_campaign_mission_id", num.ToString());
            for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; j++)
            {
                if (list[j].Objective)
                {
                    PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].SetupObjectivesCampaign(campaignId);
                }
            }
        }
        else
        {
            for (int k = 0; k < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; k++)
            {
                PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[k].SetupObjectivesProc();
            }
        }
        PandoraSingleton<MissionManager>.Instance.mapObjectiveZones = new List<MapObjectiveZone>(ground.GetComponentsInChildren<MapObjectiveZone>());
        PandoraSingleton<MissionManager>.Instance.ActivateMapObjectiveZones(activate: false);
        ICustomMissionSetup[] componentsInChildren = ground.GetComponentsInChildren<ICustomMissionSetup>();
        for (int l = 0; l < componentsInChildren.Length; l++)
        {
            componentsInChildren[l].Execute();
        }
        List<KeyValuePair<uint, uint>> objectives = PandoraSingleton<MissionStartData>.Instance.objectives;
        for (int m = 0; m < objectives.Count; m++)
        {
            for (int n = 0; n < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; n++)
            {
                for (int num2 = 0; num2 < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[n].objectives.Count; num2++)
                {
                    if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[n].objectives[num2].guid == objectives[m].Key)
                    {
                        PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[n].objectives[num2].Reload(objectives[m].Value);
                    }
                }
            }
        }
        for (int num3 = 0; num3 < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; num3++)
        {
            WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[num3];
            if (warbandController.IsPlayed())
            {
                for (int num4 = 0; num4 < warbandController.objectives.Count; num4++)
                {
                    warbandController.objectives[num4].CheckObjective();
                }
            }
            List<uint> list2 = null;
            list2 = ((warbandController.saveIdx >= PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count) ? new List<uint>() : PandoraSingleton<MissionStartData>.Instance.FightingWarbands[warbandController.saveIdx].openedSearches);
            for (int num5 = 0; num5 < list2.Count; num5++)
            {
                for (int num6 = 0; num6 < PandoraSingleton<MissionManager>.Instance.interactivePoints.Count; num6++)
                {
                    if (PandoraSingleton<MissionManager>.Instance.interactivePoints[num6].guid == list2[num5])
                    {
                        warbandController.openedSearch.Add((SearchPoint)PandoraSingleton<MissionManager>.instance.interactivePoints[num6]);
                        break;
                    }
                }
            }
        }
        for (int num7 = 0; num7 < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; num7++)
        {
            WarbandController warbandController2 = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[num7];
        }
        PandoraSingleton<MissionManager>.Instance.UpdateObjectivesUI();
    }

    private IEnumerator GenerateNavMesh()
    {
        foreach (RecastGraph rg in AstarPath.active.astarData.FindGraphsOfType(typeof(RecastGraph)))
        {
            rg.forcedBoundsSize = ground.GetComponent<BoundingBox>().size;
            rg.forcedBoundsCenter = ground.GetComponent<BoundingBox>().center;
        }
        AstarPath.active.graphUpdateBatchingInterval = 1.5f;
        foreach (Progress p in AstarPath.active.ScanAsync())
        {
            CurrentPartsPercent = p.progress;
            yield return null;
        }
        PandoraSingleton<MissionManager>.Instance.tileHandlerHelper = AstarPath.active.gameObject.AddComponent<TileHandlerHelper>();
        PandoraSingleton<MissionManager>.Instance.tileHandlerHelper.updateInterval = -1f;
        PandoraSingleton<MissionManager>.Instance.tileHandlerHelper.ForceUpdate();
        PandoraSingleton<MissionManager>.Instance.PathSeeker = AstarPath.active.gameObject.AddComponent<Seeker>();
        PandoraSingleton<MissionManager>.Instance.PathSeeker.DeregisterModifier(PandoraSingleton<MissionManager>.Instance.PathSeeker.startEndModifier);
        RayPathModifier pathModifier = AstarPath.active.gameObject.AddComponent<RayPathModifier>();
        PandoraSingleton<MissionManager>.Instance.PathSeeker.DeregisterModifier(pathModifier);
        PandoraSingleton<MissionManager>.Instance.PathSeeker.RegisterModifier(pathModifier);
        PandoraSingleton<MissionManager>.Instance.pathRayModifier = pathModifier;
        for (int z = 0; z < actionZones.Count; z++)
        {
            ActionZone zone = actionZones[z];
            for (int d = 0; d < zone.destinations.Count; d++)
            {
                ActionDestination dest = zone.destinations[d];
                uint zoneTag = 0u;
                switch (zone.destinations[d].actionId)
                {
                    case UnitActionId.CLIMB_3M:
                    case UnitActionId.CLIMB_6M:
                    case UnitActionId.CLIMB_9M:
                        zoneTag = ((!dest.destination.supportLargeUnit) ? 1u : 4u);
                        break;
                    case UnitActionId.JUMP_3M:
                    case UnitActionId.JUMP_6M:
                    case UnitActionId.JUMP_9M:
                        zoneTag = ((!dest.destination.supportLargeUnit) ? 2u : 5u);
                        break;
                    case UnitActionId.LEAP:
                        zoneTag = ((!dest.destination.supportLargeUnit) ? 3u : 6u);
                        break;
                }
                dest.navLink.startNode.Tag = zoneTag;
                dest.navLink.endNode.Tag = zoneTag;
            }
        }
        ColliderActivator.ActivateAll();
    }

    private void SetCameraSetter()
    {
        CameraSetter cameraSetter = UnityEngine.Object.FindObjectOfType<CameraSetter>();
        if ((bool)cameraSetter)
        {
            cameraSetter.SetCameraInfo(PandoraSingleton<MissionManager>.Instance.CamManager.GetComponent<Camera>());
        }
    }
}
