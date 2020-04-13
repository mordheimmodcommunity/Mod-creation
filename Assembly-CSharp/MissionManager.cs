using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : PandoraSingleton<MissionManager>
{
    public enum TurnPhase
    {
        START_OF_GAME,
        DEPLOYMENT,
        START_OF_ROUND,
        WAIT_GAME_SETUP,
        ROUT,
        UNIT_MOVEMENT,
        WATCH_UNIT,
        END_OF_ROUND,
        END_OF_GAME,
        COUNT
    }

    public const int POINTING_ARROWS_COUNT = 10;

    public const int MAPS_BEACON_MAX = 5;

    private const int MAX_EXTERNAL_PER_PRAME = 150;

    private const int MAX_ZONE_BY_FRAME = 10;

    public Color colorSelected;

    public Color colorUnselected;

    public int currentTurn;

    private int syncDone;

    public TileHandlerHelper tileHandlerHelper;

    private bool navGraphNeedsRefresh;

    [HideInInspector]
    public bool lockNavRefresh;

    [HideInInspector]
    public List<NodeLink2> nodeLinks;

    private NNConstraint nearestNodeConstraint;

    private NNInfo nearestNodeInfo;

    [HideInInspector]
    public RayPathModifier pathRayModifier;

    private List<UnitController> allEnemiesList;

    private List<UnitController> allLiveEnemiesList;

    private List<UnitController> allLiveAlliesList;

    private List<UnitController> allLiveMyUnitsList;

    private List<UnitController> allLiveUnits;

    private List<UnitController> allMyUnitsList;

    public List<UnitController> allUnitsList;

    public CampaignMissionId campaignId;

    public List<UnitController> excludedUnits;

    [HideInInspector]
    public List<ActionZone> actionZones;

    [HideInInspector]
    public List<ActionZone> accessibleActionZones = new List<ActionZone>();

    [HideInInspector]
    public List<InteractivePoint> interactivePoints;

    [HideInInspector]
    public List<InteractivePoint> interactivePointsTrash = new List<InteractivePoint>();

    private List<InteractivePoint> initialSearchPoints;

    public int numWyrdstones;

    [HideInInspector]
    public List<TriggerPoint> triggerPoints;

    private List<DecisionPoint> decisionPoints;

    private List<LocateZone> locateZones;

    [HideInInspector]
    public List<ZoneAoe> zoneAoes;

    [HideInInspector]
    public List<SpawnNode> spawnNodes;

    private Dictionary<string, PatrolRoute> patrolRoutes;

    public MissionMapData mapData;

    public MapContour mapContour;

    public MapOrigin mapOrigin;

    public List<MapObjectiveZone> mapObjectiveZones;

    public List<GameObject> pointingArrows = new List<GameObject>();

    [HideInInspector]
    public bool resendLadder;

    [HideInInspector]
    public Stack<UnitController> delayedUnits;

    [HideInInspector]
    public UnitController interruptingUnit;

    [HideInInspector]
    public List<SearchPoint> lootedEnemies = new List<SearchPoint>();

    public GameObject sphereTarget;

    private GameObject aoeTargetSphere;

    private GameObject aoeGroundTargetSphere;

    public GameObject coneTarget;

    public GameObject lineTarget;

    public GameObject dummyTargeter;

    public GameObject arcTarget;

    private List<Beacon> beacons = new List<Beacon>();

    private GameObject beaconPrefab;

    private int beaconLimit;

    private int beaconIdx;

    public GameObject deployBeaconPrefab;

    private List<MapBeacon> mapBeacons;

    [HideInInspector]
    public bool gameFinished;

    private List<ExternalUpdator> externalUpdators;

    private int extUpdaterIndex;

    private uint envGuid;

    private uint rtGuid;

    public int lastWarbandIdx;

    private GameObject lootbagPrefab;

    private bool checkMultiPlayerConnection;

    private bool checkInvite;

    [HideInInspector]
    public bool isDeploying;

    private RaycastHit hitInfo;

    private int lastBeaconIdx;

    public List<WarbandController> WarbandCtrlrs
    {
        get;
        private set;
    }

    public List<UnitController> InitiativeLadder
    {
        get;
        private set;
    }

    public CameraManager CamManager
    {
        get;
        private set;
    }

    public MessageManager MsgManager
    {
        get;
        private set;
    }

    public CheapStateMachine StateMachine
    {
        get;
        private set;
    }

    public int VictoriousTeamIdx
    {
        get;
        private set;
    }

    public MovementCircles MoveCircle
    {
        get;
        private set;
    }

    public NetworkManager NetworkMngr
    {
        get;
        private set;
    }

    public MissionEndDataSave MissionEndData
    {
        get;
        private set;
    }

    public Tyche NetworkTyche
    {
        get;
        private set;
    }

    public CombatLogger CombatLogger
    {
        get;
        private set;
    }

    public int CurrentLadderIdx
    {
        get;
        private set;
    }

    public UnitController focusedUnit
    {
        get;
        private set;
    }

    public bool transitionDone
    {
        get;
        private set;
    }

    public Seeker PathSeeker
    {
        get;
        set;
    }

    public List<MapImprint> MapImprints
    {
        get;
        private set;
    }

    public TurnTimer TurnTimer
    {
        get;
        private set;
    }

    private bool IsNavmeshWorking => AstarPath.active.isScanning || AstarPath.active.IsAnyGraphUpdateQueued || AstarPath.active.IsAnyWorkItemInProgress || AstarPath.active.IsAnyGraphUpdateInProgress;

    public bool IsNavmeshUpdating => navGraphNeedsRefresh || IsNavmeshWorking;

    private void Awake()
    {
        WarbandCtrlrs = new List<WarbandController>();
        InitiativeLadder = new List<UnitController>();
        allEnemiesList = new List<UnitController>();
        allLiveEnemiesList = new List<UnitController>();
        allLiveAlliesList = new List<UnitController>();
        allLiveMyUnitsList = new List<UnitController>();
        allLiveUnits = new List<UnitController>();
        allMyUnitsList = new List<UnitController>();
        allUnitsList = new List<UnitController>();
        MapImprints = new List<MapImprint>();
        interactivePoints = new List<InteractivePoint>();
        triggerPoints = new List<TriggerPoint>();
        decisionPoints = new List<DecisionPoint>();
        locateZones = new List<LocateZone>();
        patrolRoutes = new Dictionary<string, PatrolRoute>();
        zoneAoes = new List<ZoneAoe>();
        spawnNodes = new List<SpawnNode>();
        delayedUnits = new Stack<UnitController>();
        MsgManager = new MessageManager();
        externalUpdators = new List<ExternalUpdator>();
        extUpdaterIndex = 0;
        VictoriousTeamIdx = -1;
        numWyrdstones = 0;
        envGuid = 0u;
        rtGuid = 200000000u;
        CamManager = GameObject.Find("game_camera").GetComponent<CameraManager>();
        NetworkMngr = new NetworkManager();
        NetworkTyche = new Tyche(PandoraSingleton<MissionStartData>.Instance.Seed, log: false);
        MsgManager.Init((CampaignMissionId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId);
        CombatLogger = new CombatLogger();
        InitBeacons();
        InitTargetingAssets();
        PandoraDebug.LogInfo("Set NetworkTyche Seed = " + PandoraSingleton<MissionStartData>.Instance.Seed, "TYCHE", this);
        MissionEndData = new MissionEndDataSave();
        if (PandoraSingleton<GameManager>.Instance.currentSave != null)
        {
            PandoraSingleton<GameManager>.Instance.currentSave.endMission = MissionEndData;
        }
        MoveCircle = GetComponent<MovementCircles>();
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: false);
        StateMachine = new CheapStateMachine(9);
        StateMachine.AddState(new StartGame(this), 0);
        StateMachine.AddState(new Deployment(this), 1);
        StateMachine.AddState(new WaitGameSetup(this), 3);
        StateMachine.AddState(new StartRound(this), 2);
        StateMachine.AddState(new UnitMovement(this), 5);
        StateMachine.AddState(new WatchUnit(this), 6);
        StateMachine.AddState(new EndRound(this), 7);
        StateMachine.AddState(new Rout(this), 4);
        StateMachine.AddState(new EndGame(this), 8);
        StateMachine.SetBlockingStateIdx(8);
        PandoraSingleton<PandoraInput>.instance.SetActive(active: false);
        nearestNodeConstraint = NNConstraint.Default;
        nearestNodeConstraint.graphMask = 1;
        lastWarbandIdx = -1;
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_INVITE_ACCEPTED, InviteReceived);
    }

    public void RegisterExternalUpdator(ExternalUpdator up)
    {
        externalUpdators.Add(up);
    }

    public void ReleaseExternalUpdator(ExternalUpdator up)
    {
        externalUpdators.Remove(up);
    }

    public uint GetNextEnvGUID()
    {
        return envGuid++;
    }

    public uint GetNextRTGUID()
    {
        return rtGuid++;
    }

    public void OnDestroy()
    {
        Time.timeScale = 1f;
        StateMachine.Destroy();
        NetworkMngr.Remove();
        NetworkTyche = null;
        if (PandoraSingleton<NoticeManager>.Instance != null)
        {
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.GAME_TUTO_MESSAGE, OnTutoMessageShown);
        }
    }

    private void Start()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TRANSITION_DONE, OnTransitionDone);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_TUTO_MESSAGE, OnTutoMessageShown);
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/", AssetBundleId.PROPS, "loot_bag.prefab", delegate (UnityEngine.Object prefab)
        {
            lootbagPrefab = (GameObject)prefab;
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_pointing_range.prefab", delegate (UnityEngine.Object prefab)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject item = (GameObject)UnityEngine.Object.Instantiate(prefab);
                pointingArrows.Add(item);
            }
        });
    }

    private void Update()
    {
        if (PandoraSingleton<TransitionManager>.Instance.GameLoadingDone && PandoraSingleton<TransitionManager>.Instance.IsDone() && checkInvite)
        {
            CheckInvite();
        }
        if (checkMultiPlayerConnection && GetPlayersCount() == 2 && (!PandoraSingleton<Hermes>.Instance.IsConnected() || !PandoraSingleton<Hephaestus>.Instance.IsOnline()))
        {
            checkMultiPlayerConnection = false;
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_DONE, OnTransitionDone);
            PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone(overrideWaitPlayer: true);
            OnConnectionLost();
        }
        if (!lockNavRefresh && !IsNavmeshWorking && navGraphNeedsRefresh)
        {
            UpdateGraph();
        }
        if (TurnTimer != null)
        {
            TurnTimer.Update();
        }
        if (StateMachine.GetActiveStateId() != -1)
        {
            int count = externalUpdators.Count;
            int num = 0;
            while (num < 150 && num < count)
            {
                if (extUpdaterIndex >= count)
                {
                    extUpdaterIndex = 0;
                }
                externalUpdators[extUpdaterIndex].ExternalUpdate();
                num++;
                extUpdaterIndex++;
            }
        }
        StateMachine.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
        MapImprint mapImprint = null;
        for (int num = MapImprints.Count - 1; num >= 0; num--)
        {
            mapImprint = MapImprints[num];
            if (mapImprint != null)
            {
                if (mapImprint.UnitCtrlr == null || mapImprint.UnitCtrlr.isInLadder)
                {
                    mapImprint.RefreshPosition();
                }
            }
            else
            {
                MapImprints.RemoveAt(num);
            }
        }
        if (resendLadder)
        {
            resendLadder = false;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_CHANGED);
        }
    }

    public int GetPlayersCount()
    {
        if (WarbandCtrlrs == null)
        {
            return 0;
        }
        int num = 0;
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].playerTypeId == PlayerTypeId.PLAYER)
            {
                num++;
            }
        }
        return num;
    }

    private void InviteReceived()
    {
        checkInvite = true;
    }

    private void CheckInvite()
    {
        checkInvite = false;
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_mission_quit_tuto_title", "invite_mission_quit_tuto_desc", OnMissionReceiveInviteShouldSaveAndQuit, showCancel: true);
            return;
        }
        Warband warband = new Warband(PandoraSingleton<GameManager>.instance.currentSave);
        if (MissionEndData.isSkirmish || GetPlayersCount() == 2)
        {
            if (!warband.ValidateWarbandForInvite(inMission: false))
            {
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_skirmish_quit_title", "invite_skirmish_quit_desc", OnMissionReceiveInviteShouldSaveAndQuit, showCancel: true);
            }
            else
            {
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_skirmish_load_title", "invite_skirmish_load_desc", OnMissionReceiveInviteShouldForfeit, showCancel: true);
            }
        }
        else if (!warband.ValidateWarbandForInvite(inMission: false))
        {
            PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_mission_quit_title", "invite_mission_quit_desc", OnMissionReceiveInviteShouldSaveAndQuit, showCancel: true);
        }
        else
        {
            PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_mission_forfeit_title", "invite_mission_forfeit_desc", OnMissionReceiveInviteShouldForfeit, showCancel: true);
        }
    }

    private void OnMissionReceiveInviteShouldSaveAndQuit(bool confirm)
    {
        if (confirm)
        {
            SaveAndQuit();
        }
        else
        {
            PandoraSingleton<Hephaestus>.Instance.ResetInvite();
        }
    }

    private void OnMissionReceiveInviteShouldForfeit(bool confirm)
    {
        if (confirm)
        {
            NetworkMngr.SendForfeitMission(GetMyWarbandCtrlr().idx);
        }
        else
        {
            PandoraSingleton<Hephaestus>.Instance.ResetInvite();
        }
    }

    public void CreateMissionEndData()
    {
        MissionSave missionSave = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave;
        MissionEndData.missionSave = missionSave;
        MissionEndData.ratingId = (ProcMissionRatingId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.ratingId;
        MissionEndData.isCampaign = missionSave.isCampaign;
        MissionEndData.isSkirmish = missionSave.isSkirmish;
        MissionEndData.seed = PandoraSingleton<MissionStartData>.Instance.Seed;
        MissionEndData.missionWarbands = PandoraSingleton<MissionStartData>.Instance.FightingWarbands;
        int playersCount = GetPlayersCount();
        MissionEndData.isVsAI = (playersCount == 1);
        if (!MissionEndData.isVsAI)
        {
            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.MULTI_PLAY, 1);
        }
        List<UnitController> list = new List<UnitController>();
        list.AddRange(allUnitsList);
        list.AddRange(excludedUnits);
        MissionEndData.AddUnits(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            MissionEndData.UpdateUnit(list[i]);
        }
        MissionEndData.warbandMorals.Clear();
        for (int j = 0; j < WarbandCtrlrs.Count; j++)
        {
            Tuple<int, int, bool> item = new Tuple<int, int, bool>(WarbandCtrlrs[j].MoralValue, WarbandCtrlrs[j].OldMoralValue, WarbandCtrlrs[j].idolMoralRemoved);
            MissionEndData.warbandMorals.Add(item);
        }
        MissionEndData.destroyedTraps.Clear();
        MissionEndData.destroyedTraps.AddRange(PandoraSingleton<MissionStartData>.Instance.usedTraps);
        MissionEndData.aoeZones.Clear();
        MissionEndData.aoeZones.AddRange(PandoraSingleton<MissionStartData>.Instance.aoeZones);
        MissionEndData.objectives.Clear();
        MissionEndData.objectives.AddRange(PandoraSingleton<MissionStartData>.Instance.objectives);
        MissionEndData.converters.Clear();
        MissionEndData.converters.AddRange(PandoraSingleton<MissionStartData>.Instance.converters);
        MissionEndData.activaters.Clear();
        MissionEndData.activaters.AddRange(PandoraSingleton<MissionStartData>.Instance.activaters);
        if (PandoraSingleton<MissionStartData>.Instance.isReload)
        {
            MissionEndData.myrtilusLadder.Clear();
            MissionEndData.myrtilusLadder.AddRange(PandoraSingleton<MissionStartData>.Instance.myrtilusLadder);
            MissionEndData.currentLadderIdx = PandoraSingleton<MissionStartData>.Instance.currentLadderIdx;
            MissionEndData.currentTurn = PandoraSingleton<MissionStartData>.Instance.currentTurn;
        }
        if (!MissionEndData.missionSave.isTuto && PandoraSingleton<GameManager>.Instance.currentSave != null)
        {
            PandoraSingleton<GameManager>.Instance.currentSave.inMission = true;
            PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(PandoraSingleton<GameManager>.Instance.currentSave, PandoraSingleton<GameManager>.Instance.campaign);
        }
    }

    public void SaveAndQuit()
    {
        if (PandoraSingleton<GameManager>.Instance.currentSave != null)
        {
            PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(PandoraSingleton<GameManager>.Instance.currentSave, PandoraSingleton<GameManager>.Instance.campaign);
        }
        PandoraSingleton<Hermes>.Instance.StopConnections();
        PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME, waitForPlayers: false, force: true);
    }

    public void ForceQuitMission()
    {
        StartCoroutine(QuitMissionRoutine());
    }

    private IEnumerator QuitMissionRoutine()
    {
        yield return new WaitForFixedUpdate();
        PandoraDebug.LogDebug("Mission Manager::QuitMission");
        bool wasOnline = PandoraSingleton<Hephaestus>.Instance.IsOnline();
        bool wasConnected = PandoraSingleton<Hermes>.Instance.IsConnected();
        PandoraSingleton<Hermes>.Instance.StopConnections();
        PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
        MissionEndData.missionSave = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave;
        if (MissionEndData.missionSave.isTuto)
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME, waitForPlayers: false, force: true);
        }
        else if (StateMachine.GetActiveStateId() <= 3 && GetPlayersCount() == 2)
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_HIDEOUT, waitForPlayers: false, force: true);
        }
        else
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_END_GAME, waitForPlayers: false, force: true);
        }
    }

    public void SendLoadingDone()
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex)
            {
                NetworkMngr.SendLoadingDone(PandoraSingleton<Hermes>.Instance.PlayerIndex);
            }
        }
    }

    public void SetLoadingDone()
    {
        PandoraDebug.LogDebug("SetLoadingDone = " + (syncDone + 1) + "Numwabands = " + WarbandCtrlrs.Count, "HERMES", this);
        syncDone++;
        if (syncDone >= 2 && syncDone == WarbandCtrlrs.Count)
        {
            if (PandoraSingleton<GameManager>.Instance.IsFastForwarded)
            {
                Time.timeScale = 1.15f;
            }
            PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SYSTEM_RESUME, CheckConnection);
            PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.HERMES_CONNECTION_LOST, OnConnectionLostNoResume);
            checkMultiPlayerConnection = false;
            syncDone = 0;
            for (int i = 0; i < WarbandCtrlrs.Count; i++)
            {
                WarbandController warbandController = WarbandCtrlrs[i];
                InitiativeLadder.AddRange(warbandController.unitCtrlrs);
                for (int num = warbandController.unitCtrlrs.Count - 1; num >= 0; num--)
                {
                    UnitController unitController = warbandController.unitCtrlrs[num];
                    if (unitController.unit.CampaignData != null)
                    {
                        if (unitController.unit.CampaignData.StartHidden)
                        {
                            ExcludeUnit(unitController);
                        }
                        else if (unitController.unit.CampaignData.StartInactive)
                        {
                            RemoveUnitFromLadder(unitController);
                        }
                    }
                    if (unitController.unit.Id == UnitId.BLUE_HORROR)
                    {
                        ExcludeUnit(unitController);
                    }
                }
            }
            InitWalkability();
            StateMachine.ChangeState(0);
            PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
            PandoraSingleton<Hermes>.Instance.DoNotDisconnectMode = false;
            PandoraSingleton<FlyingTextManager>.Instance.Init();
            int num2 = 6;
            List<UnitController> allEnemies = GetAllEnemies(GetMyWarbandCtrlr().idx);
            for (int j = 0; j < allEnemies.Count; j++)
            {
                if (allEnemies[j].unit.Data.Id == UnitId.MANTICORE && num2 > 1)
                {
                    num2 = 1;
                    break;
                }
                if (num2 > 2 && allEnemies[j].unit.Data.Id == UnitId.CURATOR)
                {
                    num2 = 2;
                }
                else if (num2 > 3 && allEnemies[j].unit.Data.Id == UnitId.ALLURESS)
                {
                    num2 = 3;
                }
                else if (num2 > 3 && allEnemies[j].unit.Data.Id == UnitId.CHAOS_OGRE)
                {
                    num2 = 4;
                }
                else if (num2 > 3 && allEnemies[j].unit.RaceId == RaceId.DAEMON)
                {
                    num2 = 5;
                }
            }
            switch (num2)
            {
                case 1:
                    PandoraSingleton<Pan>.Instance.PlayMusic("boss", ambiance: true);
                    return;
                case 2:
                    PandoraSingleton<Pan>.Instance.PlayMusic("generic_01", ambiance: true);
                    return;
                case 3:
                    PandoraSingleton<Pan>.Instance.PlayMusic("generic_04", ambiance: true);
                    return;
                case 4:
                    PandoraSingleton<Pan>.Instance.PlayMusic("generic_02", ambiance: true);
                    return;
                case 5:
                    PandoraSingleton<Pan>.Instance.PlayMusic("generic_03", ambiance: true);
                    return;
            }
            bool flag = false;
            for (int k = 0; k < WarbandCtrlrs.Count; k++)
            {
                if (!WarbandCtrlrs[k].IsPlayed() && WarbandCtrlrs[k].WarData.Basic)
                {
                    flag = true;
                    PandoraSingleton<Pan>.Instance.PlayMusic(WarbandCtrlrs[k].WarData.Id.ToLowerString(), ambiance: true);
                    break;
                }
            }
            if (!flag)
            {
                PandoraSingleton<Pan>.Instance.PlayMusic(GetMyWarbandCtrlr().WarData.Id.ToLowerString(), ambiance: true);
            }
        }
        else if (GetPlayersCount() == 2)
        {
            checkMultiPlayerConnection = true;
        }
    }

    public void SendTurnReady()
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex)
            {
                NetworkMngr.SendTurnReady();
            }
        }
    }

    public void SetTurnReady()
    {
        PandoraDebug.LogDebug("SetTurnReady = " + (syncDone + 1) + "Numwabands = " + WarbandCtrlrs.Count, "HERMES", this);
        if (++syncDone == WarbandCtrlrs.Count)
        {
            syncDone = 0;
            if ((currentTurn == 0 || PandoraSingleton<MissionStartData>.Instance.isReload) && (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy))
            {
                PandoraSingleton<MissionStartData>.Instance.isReload = false;
                PandoraSingleton<UIMissionManager>.Instance.messagePopup.Hide();
            }
            else
            {
                SelectNextLadderUnit();
            }
        }
    }

    public void EndLoading()
    {
        gameFinished = false;
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
        {
            StateMachine.ChangeState(1);
            return;
        }
        StateMachine.ChangeState(3);
        PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone();
    }

    private void OnTransitionDone()
    {
        PandoraDebug.LogInfo("OnTransitionDone", "FLOW", this);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_DONE, OnTransitionDone);
        transitionDone = true;
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
        {
            StateMachine.ChangeState(2);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_ROUND_START, currentTurn);
            SelectNextLadderUnit();
            resendLadder = true;
        }
        else
        {
            StateMachine.ChangeState(1);
        }
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
    }

    public bool CheckUnitTurnFinished()
    {
        int activeStateId = GetCurrentUnit().StateMachine.GetActiveStateId();
        if (activeStateId == 39)
        {
            PandoraDebug.LogDebug("Unit " + GetCurrentUnit().name + " has finished it's turn, calling SelectNextLadderUnit.", "FLOW");
            SelectNextLadderUnit();
            return true;
        }
        return false;
    }

    public void OnTutoMessageShown()
    {
        if ((bool)PandoraSingleton<NoticeManager>.Instance.Parameters[2])
        {
            CampaignMissionData campaignMissionData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId);
            int idx = campaignMissionData.Idx - 1;
            if (PandoraSingleton<MissionStartData>.instance.CurrentMission.missionSave.isCampaign && !PandoraSingleton<MissionStartData>.instance.CurrentMission.missionSave.isTuto)
            {
                idx = 4;
            }
            PandoraSingleton<GameManager>.Instance.Profile.CompleteTutorial(idx);
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
    }

    private void OnSkirmishInviteAccepted()
    {
        if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish)
        {
            return;
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto && PandoraSingleton<GameManager>.Instance.campaign == -1)
        {
            PandoraSingleton<GameManager>.Instance.campaign = PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign;
            if (PandoraSingleton<GameManager>.Instance.campaign == -1)
            {
                PandoraSingleton<UIMissionManager>.Instance.messagePopup.Show("join_lobby_title_no_warband", "join_lobby_desc_no_warband", null);
                PandoraSingleton<UIMissionManager>.Instance.messagePopup.HideCancelButton();
            }
        }
        if (PandoraSingleton<GameManager>.Instance.campaign != -1)
        {
            PandoraSingleton<UIMissionManager>.Instance.messagePopup.Show("join_lobby_title_quit_game", "join_lobby_desc_quit_game_safe", OnQuitGameResult);
        }
    }

    private void OnQuitGameResult(bool confirm)
    {
        if (confirm)
        {
            NetworkMngr.SendForfeitMission(GetMyWarbandCtrlr().idx);
        }
    }

    private IEnumerator ConnectionLostWaitLoading(bool isResume)
    {
        while (PandoraSingleton<MissionLoader>.Exists())
        {
            yield return null;
        }
        OnConnectionLost(isResume);
    }

    public void CheckConnection()
    {
        if (GetPlayersCount() == 2 && !PandoraSingleton<Hermes>.Instance.IsConnected())
        {
            OnConnectionLost();
        }
    }

    private void OnConnectionLostNoResume()
    {
        OnConnectionLost();
    }

    public void OnConnectionLost(bool isResume = false)
    {
        if (PandoraSingleton<MissionLoader>.Exists())
        {
            StartCoroutine(ConnectionLostWaitLoading(isResume));
        }
        else if (StateMachine.GetActiveStateId() != 8)
        {
            PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
            if (!PandoraSingleton<Hephaestus>.Instance.IsOnline() || isResume)
            {
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.LOST_CONNECTION, "connection_error_title", PandoraSingleton<Hephaestus>.Instance.GetOfflineReason(), ConnectionLost);
            }
            else
            {
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.LOST_CONNECTION, "connection_error_title", "connection_error_opponent_left", ConnectionLost);
            }
        }
    }

    private void ConnectionLost(bool confirm)
    {
        if (PandoraSingleton<MissionManager>.Exists() && StateMachine != null && StateMachine.GetActiveStateId() != 8)
        {
            if (StateMachine.GetActiveStateId() <= 3)
            {
                ForceQuitMission();
            }
            else if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
            {
                List<WarbandController> enemyWarbandCtrlrs = GetEnemyWarbandCtrlrs();
                ForfeitMission(enemyWarbandCtrlrs[0].idx);
            }
            else
            {
                ForfeitMission(GetMyWarbandCtrlr().idx);
            }
        }
    }

    public void ForfeitMission(int warbandidx)
    {
        WarbandCtrlrs[warbandidx].defeated = true;
        if (WarbandCtrlrs[warbandidx].MoralRatio > PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
        {
            MissionEndData.crushed = true;
            for (int i = 0; i < WarbandCtrlrs[warbandidx].unitCtrlrs.Count; i++)
            {
                WarbandCtrlrs[warbandidx].unitCtrlrs[i].unit.CurrentWound = 0;
                WarbandCtrlrs[warbandidx].unitCtrlrs[i].unit.SetStatus(UnitStateId.OUT_OF_ACTION);
            }
        }
        if (!PandoraSingleton<SequenceManager>.Instance.isPlaying)
        {
            CheckEndGame();
        }
    }

    public bool CheckEndGame()
    {
        MissionEndData.playerMVUIdx = GetMyWarbandCtrlr().GetMVU(PandoraSingleton<GameManager>.instance.LocalTyche, enemy: false);
        MissionEndData.enemyMVUIdx = GetEnemyWarbandCtrlrs()[0].GetMVU(PandoraSingleton<GameManager>.instance.LocalTyche, enemy: true);
        Dictionary<int, List<WarbandController>> dictionary = new Dictionary<int, List<WarbandController>>();
        int num = 0;
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].IsPlayed() && WarbandCtrlrs[i].MoralRatio <= PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
            {
                MissionEndData.routable = true;
            }
            WarbandCtrlrs[i].CheckObjectives();
            if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.VictoryTypeId == 2 && WarbandCtrlrs[i].HasFailedMandatoryObjective())
            {
                WarbandCtrlrs[i].defeated = true;
            }
            if (WarbandCtrlrs[i].playerTypeId == PlayerTypeId.PLAYER)
            {
                num++;
            }
            if (WarbandCtrlrs[i].IsRoaming())
            {
                continue;
            }
            MissionEndData.crushed = false;
            if (!WarbandCtrlrs[i].defeated)
            {
                bool flag = false;
                for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
                {
                    if (WarbandCtrlrs[i].unitCtrlrs[j].unit.Status != UnitStateId.OUT_OF_ACTION)
                    {
                        flag = true;
                        break;
                    }
                }
                WarbandCtrlrs[i].defeated = (!flag || WarbandCtrlrs[i].MoralValue <= 0);
                MissionEndData.crushed |= !flag;
            }
            if (!dictionary.ContainsKey(WarbandCtrlrs[i].teamIdx))
            {
                dictionary[WarbandCtrlrs[i].teamIdx] = new List<WarbandController>();
            }
            dictionary[WarbandCtrlrs[i].teamIdx].Add(WarbandCtrlrs[i]);
        }
        int victoriousTeamIdx = -1;
        int num2 = 0;
        int num3 = 0;
        foreach (int key in dictionary.Keys)
        {
            List<WarbandController> list = dictionary[key];
            bool flag2 = true;
            for (int k = 0; k < list.Count; k++)
            {
                if (list[k].BlackList != 0)
                {
                    flag2 &= list[k].defeated;
                }
            }
            if (!flag2)
            {
                victoriousTeamIdx = key;
                num2++;
            }
            else
            {
                num3++;
            }
        }
        bool flag3 = false;
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.VictoryTypeId == 2)
        {
            for (int l = 0; l < WarbandCtrlrs.Count; l++)
            {
                if (WarbandCtrlrs[l].IsPlayed())
                {
                    if (WarbandCtrlrs[l].AllObjectivesCompleted)
                    {
                        victoriousTeamIdx = l;
                        flag3 = true;
                        break;
                    }
                    if (WarbandCtrlrs[l].defeated)
                    {
                        flag3 = true;
                        break;
                    }
                }
            }
        }
        else
        {
            flag3 = (num2 == 1 || num3 == WarbandCtrlrs.Count || (num == 1 && GetMyWarbandCtrlr().defeated));
        }
        if (flag3)
        {
            VictoriousTeamIdx = victoriousTeamIdx;
            WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
            if (myWarbandCtrlr.teamIdx == VictoriousTeamIdx)
            {
                int num4 = 0;
                bool flag4 = false;
                for (int m = 0; m < myWarbandCtrlr.unitCtrlrs.Count; m++)
                {
                    if (myWarbandCtrlr.unitCtrlrs[m].unit.Status != UnitStateId.OUT_OF_ACTION)
                    {
                        num4++;
                    }
                    flag4 |= myWarbandCtrlr.unitCtrlrs[m].unit.HasInjury(InjuryId.SEVERED_ARM);
                    flag4 |= myWarbandCtrlr.unitCtrlrs[m].unit.HasInjury(InjuryId.SEVERED_LEG);
                }
                if (num4 == 1)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.WIN_ALONE);
                }
                if (flag4)
                {
                    PandoraSingleton<Hephaestus>.instance.UnlockAchievement(Hephaestus.TrophyId.WIN_CRIPPLED);
                }
                if (!MissionEndData.isVsAI)
                {
                    PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.MULTI_WINS, 1);
                }
            }
            StateMachine.ChangeState(8);
        }
        return flag3;
    }

    public void SetTurnTimer(float turnDuration, UnityAction onDone = null)
    {
        if (onDone == null)
        {
            onDone = OnTurnTimerDone;
        }
        if (TurnTimer != null)
        {
            TurnTimer.Pause();
        }
        TurnTimer = new TurnTimer(turnDuration, onDone);
    }

    public void OnTurnTimerDone()
    {
        PandoraDebug.LogInfo("UNIT TURN FORCED TO END BY THE TIMER", "FLOW", GetCurrentUnit());
        if (IsCurrentPlayer())
        {
            GetCurrentUnit().SendSkill(SkillId.BASE_END_TURN);
        }
    }

    public void UpdateObjectivesUI(bool flyCam = false)
    {
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        float percSearch = myWarbandCtrlr.percSearch;
        float percWyrd = myWarbandCtrlr.percWyrd;
        Vector2 v = new Vector2(myWarbandCtrlr.openedSearch.Count, GetSearchPoints().Count);
        int num = 0;
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            num += WarbandCtrlrs[i].GetCollectedWyrdStone();
        }
        Vector2 v2 = new Vector2(num, numWyrdstones);
        myWarbandCtrlr.percSearch = (float)myWarbandCtrlr.openedSearch.Count / (float)GetSearchPoints().Count;
        myWarbandCtrlr.percWyrd = (float)num / (float)numWyrdstones;
        if (PandoraSingleton<MissionStartData>.Instance.isReload)
        {
            if (percSearch < 1f && myWarbandCtrlr.percSearch >= 1f)
            {
                PandoraSingleton<Pan>.Instance.Narrate("search_points4");
            }
            else if ((double)percSearch < 0.75 && (double)myWarbandCtrlr.percSearch >= 0.75)
            {
                PandoraSingleton<Pan>.Instance.Narrate("search_points3");
            }
            else if ((double)percSearch < 0.5 && (double)myWarbandCtrlr.percSearch >= 0.5)
            {
                PandoraSingleton<Pan>.Instance.Narrate("search_points2");
            }
            else if ((double)percSearch < 0.25 && (double)myWarbandCtrlr.percSearch >= 0.25)
            {
                PandoraSingleton<Pan>.Instance.Narrate("search_points1");
            }
            if (percWyrd < 1f && myWarbandCtrlr.percWyrd >= 1f)
            {
                PandoraSingleton<Pan>.Instance.Narrate("wyrdstone_claimed4");
            }
            else if ((double)percWyrd < 0.75 && (double)myWarbandCtrlr.percWyrd >= 0.75)
            {
                PandoraSingleton<Pan>.Instance.Narrate("wyrdstone_claimed3");
            }
            else if ((double)percWyrd < 0.5 && (double)myWarbandCtrlr.percWyrd >= 0.5)
            {
                PandoraSingleton<Pan>.Instance.Narrate("wyrdstone_claimed2");
            }
            else if ((double)percWyrd < 0.25 && (double)myWarbandCtrlr.percWyrd >= 0.25)
            {
                PandoraSingleton<Pan>.Instance.Narrate("wyrdstone_claimed1");
            }
        }
        if (myWarbandCtrlr != null)
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_OBJECTIVE_UPDATE, myWarbandCtrlr.objectives, v, v2);
        }
    }

    public bool IsCurrentPlayer()
    {
        UnitController currentUnit = GetCurrentUnit();
        return currentUnit != null && currentUnit.GetWarband().playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex;
    }

    public void SetDepoyLadderIndex(int index)
    {
        CurrentLadderIdx = index;
    }

    public UnitController GetCurrentUnit()
    {
        if (InitiativeLadder != null && CurrentLadderIdx >= 0 && CurrentLadderIdx < InitiativeLadder.Count)
        {
            return InitiativeLadder[CurrentLadderIdx];
        }
        return null;
    }

    public UnitController GetUnitController(Unit unit, bool includeExclude = false)
    {
        for (int i = 0; i < allUnitsList.Count; i++)
        {
            if (allUnitsList[i].unit == unit)
            {
                return allUnitsList[i];
            }
        }
        if (includeExclude)
        {
            for (int j = 0; j < excludedUnits.Count; j++)
            {
                if (excludedUnits[j].unit == unit)
                {
                    return excludedUnits[j];
                }
            }
        }
        return null;
    }

    public UnitController GetUnitController(uint uid)
    {
        for (int i = 0; i < allUnitsList.Count; i++)
        {
            if (allUnitsList[i].uid == uid)
            {
                return allUnitsList[i];
            }
        }
        return null;
    }

    public bool DissolveDeadUnits()
    {
        bool result = false;
        for (int i = 0; i < allUnitsList.Count; i++)
        {
            if (allUnitsList[i] != null && allUnitsList[i].lootBagPoint != null)
            {
                result = true;
                if (allUnitsList[i].lootBagPoint.visual != null && !allUnitsList[i].lootBagPoint.IsEmpty())
                {
                    allUnitsList[i].lootBagPoint.visual.SetActive(value: true);
                }
                allUnitsList[i].Imprint.alwaysHide = true;
                allUnitsList[i].Hide(hide: true);
            }
        }
        return result;
    }

    public void ExcludeUnit(UnitController ctrlr)
    {
        excludedUnits.Add(ctrlr);
        ctrlr.GetWarband().unitCtrlrs.Remove(ctrlr);
        allUnitsList.Remove(ctrlr);
        RemoveUnitFromLadder(ctrlr);
        ResetAllUnitsTargetData();
        ctrlr.gameObject.SetActive(value: false);
    }

    public void IncludeUnit(UnitController ctrlr, Vector3 pos, Quaternion rot)
    {
        IncludeUnit(ctrlr, WarbandCtrlrs[ctrlr.unit.warbandIdx], pos, rot);
    }

    public void IncludeUnit(UnitController ctrlr, WarbandController warCtrlr, Vector3 pos, Quaternion rot)
    {
        ctrlr.gameObject.SetActive(value: true);
        ctrlr.Imprint.alwaysHide = false;
        excludedUnits.Remove(ctrlr);
        warCtrlr.unitCtrlrs.Add(ctrlr);
        MissionEndData.UpdateUnit(ctrlr);
        allUnitsList.Add(ctrlr);
        AddUnitToLadder(ctrlr);
        ResetAllUnitsTargetData();
        ctrlr.transform.position = pos;
        ctrlr.transform.rotation = rot;
        ctrlr.StartGameInitialization();
        ctrlr.Deployed();
    }

    public void ResetAllUnitsTargetData()
    {
        for (int i = 0; i < allUnitsList.Count; i++)
        {
            allUnitsList[i].InitTargetsData();
        }
    }

    public void ForceUnitVisibilityCheck(UnitController unitCtrlr)
    {
        List<UnitController> myAliveUnits = GetMyAliveUnits();
        for (int i = 0; i < myAliveUnits.Count; i++)
        {
            myAliveUnits[i].UpdateTargetData(unitCtrlr);
        }
        RefreshFoWTargetMoving(unitCtrlr);
    }

    public void RegisterInteractivePoint(InteractivePoint point)
    {
        interactivePoints.Add(point);
    }

    public List<InteractivePoint> GetSearchPoints()
    {
        if (initialSearchPoints == null)
        {
            initialSearchPoints = new List<InteractivePoint>();
            for (int i = 0; i < interactivePoints.Count; i++)
            {
                if (interactivePoints[i] != null && interactivePoints[i] is SearchPoint && !((SearchPoint)interactivePoints[i]).isWyrdstone)
                {
                    initialSearchPoints.Add(interactivePoints[i]);
                }
            }
        }
        return initialSearchPoints;
    }

    public List<SearchPoint> GetWyrdstonePoints()
    {
        List<SearchPoint> list = new List<SearchPoint>();
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            if (interactivePoints[i] is SearchPoint && ((SearchPoint)interactivePoints[i]).isWyrdstone)
            {
                list.Add((SearchPoint)interactivePoints[i]);
            }
        }
        return list;
    }

    public int GetInitialWyrdstoneCount()
    {
        int num = 0;
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            if (!(interactivePoints[i] is SearchPoint))
            {
                continue;
            }
            SearchPoint searchPoint = (SearchPoint)interactivePoints[i];
            for (int j = 0; j < searchPoint.items.Count; j++)
            {
                if (searchPoint.items[j].IsWyrdStone)
                {
                    num++;
                }
            }
        }
        return num;
    }

    public void GetUnclaimedLootableItems(ref List<Item> wyrdstones, ref List<Item> search)
    {
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            SearchPoint searchPoint = interactivePoints[i] as SearchPoint;
            if (!(searchPoint != null) || searchPoint.warbandController != null || !(searchPoint.unitController == null) || searchPoint.IsEmpty())
            {
                continue;
            }
            for (int j = 0; j < searchPoint.items.Count; j++)
            {
                if (searchPoint.items[j].TypeData.Id != ItemTypeId.QUEST_ITEM && searchPoint.items[j].Id != 0)
                {
                    if (searchPoint.items[j].IsWyrdStone)
                    {
                        wyrdstones.Add(searchPoint.items[j]);
                    }
                    else
                    {
                        search.Add(searchPoint.items[j]);
                    }
                }
            }
        }
    }

    public void UnregisterInteractivePoint(InteractivePoint point)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                WarbandCtrlrs[i].unitCtrlrs[j].interactivePoints.Remove(point);
                if (WarbandCtrlrs[i].unitCtrlrs[j].AICtrlr != null && point is SearchPoint && WarbandCtrlrs[i].unitCtrlrs[j].AICtrlr.targetSearchPoint == (SearchPoint)point)
                {
                    WarbandCtrlrs[i].unitCtrlrs[j].AICtrlr.targetSearchPoint = null;
                }
            }
        }
        interactivePoints.Remove(point);
        interactivePointsTrash.Add(point);
        if (!(point is SearchPoint))
        {
            return;
        }
        SearchPoint searchPoint = (SearchPoint)point;
        for (int k = 0; k < MissionEndData.searches.Count; k++)
        {
            if (MissionEndData.searches[k].Key == searchPoint.guid)
            {
                SearchSave value = MissionEndData.searches[k].Value;
                value.items = null;
                MissionEndData.searches[k] = new KeyValuePair<uint, SearchSave>(searchPoint.guid, value);
            }
        }
    }

    public int GetInteractivePointIndex(InteractivePoint point)
    {
        return interactivePoints.IndexOf(point);
    }

    public InteractivePoint GetInteractivePoint(int index)
    {
        return interactivePoints[index];
    }

    public List<SearchPoint> GetSearchPoints(string name)
    {
        List<SearchPoint> list = new List<SearchPoint>();
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            if (interactivePoints[i] != null && interactivePoints[i].name == name && interactivePoints[i] is SearchPoint)
            {
                list.Add((SearchPoint)interactivePoints[i]);
            }
        }
        for (int j = 0; j < interactivePointsTrash.Count; j++)
        {
            if (interactivePointsTrash[j] != null && interactivePointsTrash[j].name == name && interactivePointsTrash[j] is SearchPoint)
            {
                list.Add((SearchPoint)interactivePointsTrash[j]);
            }
        }
        return list;
    }

    public List<ActivatePoint> GetActivatePoints(string name)
    {
        List<ActivatePoint> list = new List<ActivatePoint>();
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            if (interactivePoints[i] != null && interactivePoints[i] is ActivatePoint && interactivePoints[i].name == name)
            {
                list.Add((ActivatePoint)interactivePoints[i]);
            }
        }
        return list;
    }

    public List<SearchPoint> GetSearchPointInRadius(Vector3 pos, float dist, UnitActionId actionId)
    {
        List<SearchPoint> list = new List<SearchPoint>();
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            if (interactivePoints[i] != null && interactivePoints[i].unitActionId == actionId && interactivePoints[i] is SearchPoint && !((SearchPoint)interactivePoints[i]).IsEmpty() && Vector3.SqrMagnitude(pos - interactivePoints[i].transform.position) < dist * dist)
            {
                list.Add((SearchPoint)interactivePoints[i]);
            }
        }
        return list;
    }

    public List<Destructible> GetDestructibles(string name)
    {
        List<Destructible> list = new List<Destructible>();
        for (int i = 0; i < triggerPoints.Count; i++)
        {
            if (triggerPoints[i] != null && triggerPoints[i] is Destructible && triggerPoints[i].name == name)
            {
                list.Add((Destructible)triggerPoints[i]);
            }
        }
        return list;
    }

    public SearchPoint SpawnLootBag(UnitController owner, Vector3 pos, List<ItemSave> itemSaves, bool visible, bool wasSearched)
    {
        List<Item> list = new List<Item>();
        for (int i = 0; i < itemSaves.Count; i++)
        {
            list.Add(new Item(itemSaves[i]));
        }
        return SpawnLootBag(owner, pos, list, visible, wasSearched);
    }

    public SearchPoint SpawnLootBag(UnitController owner, Vector3 pos, List<Item> items, bool visible, bool wasSearched)
    {
        if (lootbagPrefab == null)
        {
            lootbagPrefab = PandoraSingleton<AssetBundleLoader>.Instance.LoadAsset<GameObject>("Assets/prefabs/environments/props/", AssetBundleId.PROPS, "loot_bag.prefab");
        }
        GameObject gameObject = UnityEngine.Object.Instantiate(lootbagPrefab);
        gameObject.name = "loot_" + owner.unit.Name;
        gameObject.transform.position = pos;
        gameObject.transform.rotation = Quaternion.identity;
        SearchPoint componentInChildren = gameObject.GetComponentInChildren<SearchPoint>();
        componentInChildren.unitController = owner;
        componentInChildren.loc_name = "search_body";
        componentInChildren.Init(GetNextRTGUID());
        componentInChildren.visual.SetActive(visible);
        for (int i = 0; i < items.Count; i++)
        {
            ResetItemOwnership(items[i], owner);
            componentInChildren.AddItem(items[i]);
        }
        owner.lootBagPoint = componentInChildren;
        MissionEndData.UpdateSearches(componentInChildren.guid, owner.uid, pos, items.ConvertAll((Item x) => x.Save), wasSearched);
        return componentInChildren;
    }

    public List<Item> FindObjectivesInSearch(ItemId itemId)
    {
        List<Item> itemsFound = new List<Item>();
        ExtractItemsFromSearch(interactivePoints, itemId, ref itemsFound);
        ExtractItemsFromSearch(interactivePointsTrash, itemId, ref itemsFound);
        return itemsFound;
    }

    private void ExtractItemsFromSearch(List<InteractivePoint> points, ItemId itemId, ref List<Item> itemsFound)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (!(points[i] != null) || !(points[i] is SearchPoint))
            {
                continue;
            }
            List<Item> objectiveItems = ((SearchPoint)points[i]).GetObjectiveItems();
            for (int j = 0; j < objectiveItems.Count; j++)
            {
                if (objectiveItems[j].Id == itemId)
                {
                    itemsFound.Add(objectiveItems[j]);
                }
            }
        }
    }

    public void FindObjectiveInUnits(ItemId itemId, ref List<Item> foundItems)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                for (int k = 0; k < WarbandCtrlrs[i].unitCtrlrs[j].unit.Items.Count; k++)
                {
                    if (WarbandCtrlrs[i].unitCtrlrs[j].unit.Items[k].Id == itemId)
                    {
                        foundItems.Add(WarbandCtrlrs[i].unitCtrlrs[j].unit.Items[k]);
                    }
                }
            }
        }
    }

    public void RestoreUnitWeapons()
    {
        for (int i = 0; i < interactivePoints.Count; i++)
        {
            SearchPoint searchPoint = interactivePoints[i] as SearchPoint;
            if (!(searchPoint != null) || !(searchPoint.unitController != null))
            {
                continue;
            }
            for (int j = 0; j < searchPoint.items.Count; j++)
            {
                Item item = searchPoint.items[j];
                if (item.owner == searchPoint.unitController.unit && (item.Save.oldSlot == 2 || item.Save.oldSlot == 3 || item.Save.oldSlot == 4 || item.Save.oldSlot == 5))
                {
                    searchPoint.SetItem(j, ItemId.NONE);
                    item.owner.EquipItem((UnitSlotId)item.Save.oldSlot, item);
                }
            }
            searchPoint.SortItems();
            searchPoint.Close(force: true);
            MissionEndData.UpdateUnit(searchPoint.unitController);
        }
    }

    public void ResetItemOwnership(Item item, UnitController owner)
    {
        if (owner != null && item.Save.ownerMyrtilus == owner.uid)
        {
            item.owner = owner.unit;
        }
        if (item.Save.ownerMyrtilus >= 200000000)
        {
            uint num = item.Save.ownerMyrtilus - 200000000;
            for (int i = 0; i < WarbandCtrlrs.Count; i++)
            {
                if (WarbandCtrlrs[i].saveIdx == num)
                {
                    WarbandCtrlrs[i].ItemIdol = item;
                    break;
                }
            }
        }
        else if (item.Save.ownerMyrtilus != 0)
        {
            for (int j = 0; j < allUnitsList.Count; j++)
            {
                if (item.Save.ownerMyrtilus == allUnitsList[j].uid)
                {
                    item.owner = allUnitsList[j].unit;
                    break;
                }
            }
        }
        else
        {
            item.owner = ((!(owner != null)) ? null : owner.unit);
            item.Save.ownerMyrtilus = ((owner != null) ? owner.uid : 0u);
        }
        if (!item.IsTrophy)
        {
            return;
        }
        for (int k = 0; k < allUnitsList.Count; k++)
        {
            if (allUnitsList[k].unit == item.owner)
            {
                allUnitsList[k].unit.deathTrophy = item;
            }
        }
    }

    public void RegisterDecisionPoint(DecisionPoint point)
    {
        decisionPoints.Add(point);
    }

    public List<DecisionPoint> GetDecisionPoints(UnitController target, DecisionPointId pointId, float distance, bool excludeCloseToUnits = true)
    {
        List<DecisionPoint> list = new List<DecisionPoint>();
        List<UnitController> allAliveUnits = GetAllAliveUnits();
        float @float = Constant.GetFloat(ConstantId.MELEE_RANGE_NORMAL);
        @float *= @float;
        float float2 = Constant.GetFloat(ConstantId.MELEE_RANGE_LARGE);
        float2 *= float2;
        for (int i = 0; i < decisionPoints.Count; i++)
        {
            if (decisionPoints[i].id != pointId || !(Vector3.SqrMagnitude(decisionPoints[i].transform.position - target.transform.position) < distance * distance))
            {
                continue;
            }
            bool flag = true;
            if (excludeCloseToUnits)
            {
                for (int j = 0; j < allAliveUnits.Count; j++)
                {
                    if (Vector3.SqrMagnitude(allAliveUnits[j].transform.position - decisionPoints[i].transform.position) < ((allAliveUnits[j].unit.Data.UnitSizeId != UnitSizeId.LARGE) ? @float : float2))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                list.Add(decisionPoints[i]);
            }
        }
        return list;
    }

    public List<DecisionPoint> GetAvailableSpawnPoints(bool visible, bool asc = false, Transform referencePoint = null, List<DecisionPoint> forcedSpawnPoints = null)
    {
        List<UnitController> allAliveUnits = GetAllAliveUnits();
        List<UnitController> unitCtrlrs = GetMyWarbandCtrlr().unitCtrlrs;
        List<DecisionPoint> list = new List<DecisionPoint>();
        List<DecisionPoint> list2 = (forcedSpawnPoints == null) ? decisionPoints : forcedSpawnPoints;
        list.AddRange(list2.FindAll((DecisionPoint x) => x.id == DecisionPointId.SPAWN));
        for (int num = list.Count - 1; num >= 0; num--)
        {
            bool flag = visible;
            for (int i = 0; i < unitCtrlrs.Count; i++)
            {
                UnitController unitController = unitCtrlrs[i];
                Vector3 position = unitController.transform.position;
                position.y += 1.5f;
                Vector3 position2 = list[num].transform.position;
                position2.y += 1.25f;
                Physics.Raycast(position, Vector3.Normalize(position2 - position), out hitInfo, unitController.unit.ViewDistance, LayerMaskManager.fowMask);
                float num2 = Vector3.Distance(position, position2);
                bool flag2 = hitInfo.collider == null || hitInfo.distance > num2;
                if ((!visible && flag2) || (visible && flag2))
                {
                    flag = !visible;
                    break;
                }
            }
            if (flag)
            {
                list.RemoveAt(num);
            }
        }
        if (list.Count == 0)
        {
            list.AddRange(decisionPoints.FindAll((DecisionPoint x) => x.id == DecisionPointId.SPAWN));
        }
        for (int num3 = list.Count - 1; num3 >= 0; num3--)
        {
            bool flag3 = true;
            list[num3].closestUnitSqrDist = float.PositiveInfinity;
            for (int j = 0; j < allAliveUnits.Count; j++)
            {
                if (!flag3)
                {
                    break;
                }
                if (allAliveUnits[j].isInLadder)
                {
                    float num4 = Vector3.SqrMagnitude(list[num3].transform.position - allAliveUnits[j].transform.position);
                    if (referencePoint == null)
                    {
                        list[num3].closestUnitSqrDist = Mathf.Min(list[num3].closestUnitSqrDist, num4);
                    }
                    flag3 = (num4 > 6.25f);
                }
            }
            if (!flag3)
            {
                list.RemoveAt(num3);
            }
            else if (referencePoint != null)
            {
                list[num3].closestUnitSqrDist = Vector3.SqrMagnitude(list[num3].transform.position - referencePoint.position);
            }
        }
        int mod = asc ? 1 : (-1);
        list.Sort((DecisionPoint x, DecisionPoint y) => x.closestUnitSqrDist.CompareTo(y.closestUnitSqrDist) * mod);
        return list;
    }

    public void RegisterLocateZone(LocateZone zone)
    {
        locateZones.Add(zone);
    }

    public List<LocateZone> GetLocateZones(string name)
    {
        List<LocateZone> list = new List<LocateZone>();
        for (int i = 0; i < locateZones.Count; i++)
        {
            if (locateZones[i].name == name)
            {
                list.Add(locateZones[i]);
            }
        }
        return list;
    }

    public List<LocateZone> GetLocateZones()
    {
        return locateZones;
    }

    public void RegisterPatrolRoute(PatrolRoute route)
    {
        patrolRoutes[route.name] = route;
    }

    public void RegisterZoneAoe(ZoneAoe zone)
    {
        zoneAoes.Add(zone);
    }

    public void UpdateZoneAoeDurations(UnitController unitCtrlr)
    {
        for (int i = 0; i < zoneAoes.Count; i++)
        {
            if (zoneAoes[i].Owner == unitCtrlr)
            {
                zoneAoes[i].UpdateDuration();
            }
        }
    }

    public void ClearZoneAoes()
    {
        for (int num = zoneAoes.Count - 1; num >= 0; num--)
        {
            if (!zoneAoes[num].gameObject.activeSelf && !zoneAoes[num].indestructible)
            {
                UnityEngine.Object.Destroy(zoneAoes[num].gameObject);
                zoneAoes.RemoveAt(num);
            }
        }
    }

    public int ZoneAoeIdx(ZoneAoe zone)
    {
        return zoneAoes.IndexOf(zone);
    }

    public ZoneAoe GetZoneAoe(int idx)
    {
        if (idx >= 0 && idx < zoneAoes.Count)
        {
            return zoneAoes[idx];
        }
        return null;
    }

    public WarbandController GetMyWarbandCtrlr()
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex && WarbandCtrlrs[i].playerTypeId == PlayerTypeId.PLAYER)
            {
                return WarbandCtrlrs[i];
            }
        }
        return WarbandCtrlrs[0];
    }

    public int GetCampaignWarbandIdx(CampaignWarbandId campWarbandId)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].CampaignWarbandId == campWarbandId)
            {
                return i;
            }
        }
        return -1;
    }

    public List<WarbandController> GetEnemyWarbandCtrlrs()
    {
        List<WarbandController> list = new List<WarbandController>();
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].teamIdx != myWarbandCtrlr.teamIdx)
            {
                list.Add(WarbandCtrlrs[i]);
            }
        }
        return list;
    }

    public WarbandController GetMainEnemyWarbandCtrlr()
    {
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        WarbandController warbandController = null;
        int num = 0;
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].teamIdx != myWarbandCtrlr.teamIdx && WarbandCtrlrs[i].MaxMoralValue > num)
            {
                warbandController = WarbandCtrlrs[i];
                num = warbandController.MaxMoralValue;
            }
        }
        return warbandController;
    }

    public List<UnitController> GetAllMyUnits()
    {
        allMyUnitsList.Clear();
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        for (int i = 0; i < myWarbandCtrlr.unitCtrlrs.Count; i++)
        {
            allMyUnitsList.Add(myWarbandCtrlr.unitCtrlrs[i]);
        }
        return allMyUnitsList;
    }

    public List<UnitController> GetMyAliveUnits()
    {
        allLiveMyUnitsList.Clear();
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        for (int i = 0; i < myWarbandCtrlr.unitCtrlrs.Count; i++)
        {
            if (myWarbandCtrlr.unitCtrlrs[i].unit.Status != UnitStateId.OUT_OF_ACTION)
            {
                allLiveMyUnitsList.Add(myWarbandCtrlr.unitCtrlrs[i]);
            }
        }
        return allLiveMyUnitsList;
    }

    public List<UnitController> GetAliveAllies(int warbandIdx)
    {
        allLiveAlliesList.Clear();
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].teamIdx != WarbandCtrlrs[warbandIdx].teamIdx)
            {
                continue;
            }
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                if (WarbandCtrlrs[i].unitCtrlrs[j].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    allLiveAlliesList.Add(WarbandCtrlrs[i].unitCtrlrs[j]);
                }
            }
        }
        return allLiveAlliesList;
    }

    public List<UnitController> GetAllEnemies(int warbandIdx)
    {
        allEnemiesList.Clear();
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].idx != warbandIdx && WarbandCtrlrs[i].teamIdx != WarbandCtrlrs[warbandIdx].teamIdx && WarbandCtrlrs[warbandIdx].BlackListed(WarbandCtrlrs[i].idx))
            {
                allEnemiesList.AddRange(WarbandCtrlrs[i].unitCtrlrs);
            }
        }
        return allEnemiesList;
    }

    public List<UnitController> GetAliveEnemies(int warbandIdx)
    {
        allLiveEnemiesList.Clear();
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            if (WarbandCtrlrs[i].idx == warbandIdx || WarbandCtrlrs[i].teamIdx == WarbandCtrlrs[warbandIdx].teamIdx || !WarbandCtrlrs[warbandIdx].BlackListed(WarbandCtrlrs[i].idx))
            {
                continue;
            }
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                if (WarbandCtrlrs[i].unitCtrlrs[j].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    allLiveEnemiesList.Add(WarbandCtrlrs[i].unitCtrlrs[j]);
                }
            }
        }
        return allLiveEnemiesList;
    }

    public List<UnitController> GetAllUnits()
    {
        return allUnitsList;
    }

    public List<UnitController> GetAllAliveUnits()
    {
        allLiveUnits.Clear();
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                if (WarbandCtrlrs[i].unitCtrlrs[j] != null && WarbandCtrlrs[i].unitCtrlrs[j].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    allLiveUnits.Add(WarbandCtrlrs[i].unitCtrlrs[j]);
                }
            }
        }
        return allLiveUnits;
    }

    public UnitController OwnUnitInvolved(UnitController unit1, UnitController unit2)
    {
        if (unit1 != null && unit1.GetWarband().playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex && unit1.AICtrlr == null)
        {
            return unit1;
        }
        if (unit2 != null && unit2.GetWarband().playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex && unit2.AICtrlr == null)
        {
            return unit2;
        }
        return null;
    }

    public int GetLadderLastValidPosition()
    {
        int result = 0;
        for (int i = 0; i < InitiativeLadder.Count; i++)
        {
            if (InitiativeLadder[i].unit.Status != UnitStateId.OUT_OF_ACTION)
            {
                result = i;
            }
        }
        return result;
    }

    public void SendUnitBack(int positionCount)
    {
        int index = Mathf.Min(CurrentLadderIdx + positionCount + 1, InitiativeLadder.Count);
        UnitController currentUnit = GetCurrentUnit();
        InitiativeLadder.Insert(index, currentUnit);
        InitiativeLadder.RemoveAt(CurrentLadderIdx);
        SaveLadder();
        SelectNextLadderUnit(0);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_CHANGED);
    }

    public void ResetLadder()
    {
        CurrentLadderIdx = -1;
        int count = InitiativeLadder.Count;
        for (int i = 0; i < count; i++)
        {
            InitiativeLadder[i].ladderVisible = (InitiativeLadder[i].unit.Status != UnitStateId.OUT_OF_ACTION);
        }
    }

    public void ResetLadderIdx(bool updateUI = true)
    {
        ResetLadder();
        InitiativeLadder.Sort(new LadderSorter());
        if (updateUI)
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_CHANGED);
        }
        SaveLadder();
    }

    private void SaveLadder()
    {
        MissionEndData.myrtilusLadder.Clear();
        for (int i = 0; i < InitiativeLadder.Count; i++)
        {
            MissionEndData.myrtilusLadder.Add(InitiativeLadder[i].uid);
        }
        MissionEndData.currentLadderIdx = CurrentLadderIdx;
        MissionEndData.currentTurn = currentTurn;
    }

    public void ReloadLadder()
    {
        List<uint> myrtilusLadder = PandoraSingleton<MissionStartData>.Instance.myrtilusLadder;
        for (int i = 0; i < myrtilusLadder.Count; i++)
        {
            UnitController unitController = null;
            for (int j = 0; j < excludedUnits.Count; j++)
            {
                if (myrtilusLadder[i] == excludedUnits[j].uid)
                {
                    unitController = excludedUnits[j];
                    break;
                }
            }
            if (unitController == null)
            {
                for (int k = 0; k < allUnitsList.Count; k++)
                {
                    if (myrtilusLadder[i] == allUnitsList[k].uid)
                    {
                        unitController = allUnitsList[k];
                        break;
                    }
                }
            }
            if (unitController != null && !unitController.isInLadder)
            {
                if (allUnitsList.IndexOf(unitController) == -1)
                {
                    IncludeUnit(unitController, unitController.transform.position, unitController.transform.rotation);
                }
                else
                {
                    AddUnitToLadder(unitController);
                }
            }
        }
        InitiativeLadder = new List<UnitController>(myrtilusLadder.Count);
        for (int l = 0; l < myrtilusLadder.Count; l++)
        {
            for (int m = 0; m < allUnitsList.Count; m++)
            {
                if (myrtilusLadder[l] == allUnitsList[m].uid)
                {
                    InitiativeLadder.Add(allUnitsList[m]);
                    break;
                }
            }
        }
        CurrentLadderIdx = -1;
        if (PandoraSingleton<MissionStartData>.Instance.currentLadderIdx > -1)
        {
            CurrentLadderIdx = PandoraSingleton<MissionStartData>.Instance.currentLadderIdx - 1;
            if (CurrentLadderIdx >= -1)
            {
                InitiativeLadder[CurrentLadderIdx + 1].TurnStarted = true;
            }
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_CHANGED);
        SaveLadder();
    }

    public void RemoveUnitFromLadder(UnitController ctrlr)
    {
        InitiativeLadder.Remove(ctrlr);
        ctrlr.Imprint.alwaysVisible = false;
        ctrlr.Imprint.alwaysHide = true;
        ctrlr.Imprint.Hide();
        ctrlr.isInLadder = false;
        ctrlr.Hide(hide: true, force: true);
        SaveLadder();
    }

    public void AddUnitToLadder(UnitController ctrlr)
    {
        if (InitiativeLadder.IndexOf(ctrlr) == -1)
        {
            InitiativeLadder.Add(ctrlr);
        }
        ctrlr.Imprint.alwaysVisible = ctrlr.IsPlayed();
        ctrlr.Imprint.alwaysHide = false;
        ctrlr.isInLadder = true;
        ctrlr.ladderVisible = true;
        resendLadder = true;
        ctrlr.Hide(ctrlr.IsPlayed(), force: true);
        SaveLadder();
    }

    public void SelectNextLadderUnit(int modifier = 1)
    {
        PandoraDebug.LogDebug("SelectNextLadderUnit", "FLOW", this);
        MoveCircle.Hide();
        if (CheckEndGame())
        {
            return;
        }
        if (CurrentLadderIdx != -1)
        {
            lastWarbandIdx = InitiativeLadder[CurrentLadderIdx].GetWarband().idx;
        }
        else
        {
            lastWarbandIdx = -1;
        }
        CurrentLadderIdx += modifier;
        if (CurrentLadderIdx < InitiativeLadder.Count)
        {
            UnitController currentUnit = InitiativeLadder[CurrentLadderIdx];
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, currentUnit);
            MissionEndData.currentLadderIdx = CurrentLadderIdx;
            if (currentUnit.TurnStarted)
            {
                SetCombatCircles(currentUnit, delegate
                {
                    TurnTimer.Reset(currentUnit.lastTimer);
                    currentUnit.lastTimer = 0f;
                    currentUnit.nextState = UnitController.State.START_MOVE;
                    WatchOrMove();
                });
            }
            else
            {
                StateMachine.ChangeState(4);
            }
        }
        else
        {
            StateMachine.ChangeState(7);
        }
    }

    public UnitController GetLastPlayedAliveUnit(int warbandIdx)
    {
        for (int num = CurrentLadderIdx - 1; num >= 0; num--)
        {
            UnitController unitController = InitiativeLadder[num];
            if (unitController.unit.warbandIdx == warbandIdx && unitController.unit.Status != UnitStateId.OUT_OF_ACTION)
            {
                return unitController;
            }
        }
        return null;
    }

    public void WatchOrMove()
    {
        if (GetCurrentUnit().IsPlayed())
        {
            if (StateMachine.GetActiveStateId() != 5)
            {
                CamManager.Locked = false;
                StateMachine.ChangeState(5);
            }
            return;
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_SHOW_ENEMY, v1: true);
        if (StateMachine.GetActiveStateId() != 6)
        {
            CamManager.Locked = true;
            StateMachine.ChangeState(6);
        }
    }

    private void InitWalkability()
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                WarbandCtrlrs[i].unitCtrlrs[j].SetGraphWalkability(walkable: false);
            }
        }
    }

    public void RefreshGraph()
    {
        navGraphNeedsRefresh = true;
    }

    private void UpdateGraph()
    {
        PandoraDebug.LogDebug("UpdateGraph", "MISSION MANAGER");
        navGraphNeedsRefresh = false;
        float floatSqr = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_LARGE);
        float floatSqr2 = Constant.GetFloatSqr(ConstantId.MELEE_RANGE_NORMAL);
        for (int i = 0; i < nodeLinks.Count; i++)
        {
            NodeLink2 nodeLink = nodeLinks[i];
            nodeLink.startNode.Walkable = true;
            nodeLink.endNode.Walkable = true;
            nodeLink.oneWay = true;
            for (int j = 0; j < WarbandCtrlrs.Count; j++)
            {
                WarbandController warbandController = WarbandCtrlrs[j];
                for (int k = 0; k < warbandController.unitCtrlrs.Count; k++)
                {
                    UnitController unitController = warbandController.unitCtrlrs[k];
                    if (unitController != GetCurrentUnit() && unitController.unit.Status != UnitStateId.OUT_OF_ACTION)
                    {
                        float num = (unitController.unit.Data.UnitSizeId != UnitSizeId.LARGE) ? floatSqr2 : floatSqr;
                        if (Vector3.SqrMagnitude(unitController.transform.position - nodeLink.StartTransform.position) < num || Vector3.SqrMagnitude(unitController.transform.position - nodeLink.EndTransform.position) < num)
                        {
                            nodeLink.startNode.Walkable = false;
                            nodeLink.endNode.Walkable = false;
                        }
                    }
                }
            }
        }
        tileHandlerHelper.ForceUpdate();
    }

    public Vector3 ClampToNavMesh(Vector3 pos)
    {
        if (!IsOnNavmesh(pos))
        {
            nearestNodeInfo = AstarPath.active.GetNearest(pos + Vector3.up * 0.07f, nearestNodeConstraint);
            Vector3 position = nearestNodeInfo.position;
            float x = position.x;
            float y = pos.y;
            Vector3 position2 = nearestNodeInfo.position;
            return new Vector3(x, y, position2.z);
        }
        return pos;
    }

    public bool IsOnNavmesh(Vector3 pos)
    {
        return AstarPath.active.astarData.recastGraph.PointOnNavmesh(pos, nearestNodeConstraint) != null;
    }

    private void CheckFoWVisibility(UnitController ctrlr, MapImprint imprint)
    {
        if (imprint == null)
        {
            return;
        }
        Transform transform = imprint.transform;
        Vector3 position = ctrlr.transform.position;
        position.y += 1.5f;
        Vector3 position2 = transform.position;
        position2.y += 1.25f;
        float num = Vector3.SqrMagnitude(position - position2);
        if (num >= (float)(ctrlr.unit.ViewDistance * ctrlr.unit.ViewDistance))
        {
            imprint.RemoveViewer(ctrlr);
        }
        else if (imprint.UnitCtrlr != null)
        {
            if (ctrlr.IsInRange(imprint.UnitCtrlr, 0f, ctrlr.unit.ViewDistance, Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC), unitBlocking: false, checkAllBones: false, BoneId.NONE))
            {
                imprint.AddViewer(ctrlr);
                if (imprint.UnitCtrlr.AICtrlr != null)
                {
                    imprint.UnitCtrlr.GetWarband().SquadManager.UnitSpotted(ctrlr);
                    imprint.UnitCtrlr.AICtrlr.hasSeenEnemy = true;
                }
                if (imprint.UnitCtrlr != null && !imprint.UnitCtrlr.isInLadder)
                {
                    AddUnitToLadder(imprint.UnitCtrlr);
                }
            }
            else
            {
                imprint.RemoveViewer(ctrlr);
            }
        }
        else if (imprint.Flag == MapImprint.currentFlagChecked)
        {
            int num2 = Physics.RaycastNonAlloc(position, position2 - position, PandoraUtils.hits, ctrlr.unit.ViewDistance, LayerMaskManager.fowMask);
            Collider collider = PandoraUtils.hits[0].collider;
            if (num2 == 0 || collider == null || (collider != null && collider.transform == transform) || (collider != null && collider.transform.parent != null && collider.transform.parent == transform) || PandoraUtils.hits[0].distance * PandoraUtils.hits[0].distance > num)
            {
                imprint.AddViewer(ctrlr);
            }
            else
            {
                imprint.RemoveViewer(ctrlr);
            }
        }
    }

    public void InitFoW()
    {
        List<UnitController> allMyUnits = GetAllMyUnits();
        for (int i = 0; i < MapImprints.Count; i++)
        {
            for (int j = 0; j < allMyUnits.Count; j++)
            {
                if (allMyUnits[j].isInLadder && MapImprints[i].UnitCtrlr != allMyUnits[j])
                {
                    allMyUnits[j].UpdateTargetsData();
                    if (!MapImprints[i].alwaysVisible && !MapImprints[i].alwaysHide)
                    {
                        CheckFoWVisibility(allMyUnits[j], MapImprints[i]);
                    }
                }
            }
            MapImprints[i].needsRefresh = true;
        }
    }

    public void RefreshFoWOwnMoving(UnitController ctrlr)
    {
        for (int i = 0; i < MapImprints.Count; i++)
        {
            if (MapImprints[i].UnitCtrlr == null || !MapImprints[i].UnitCtrlr.IsPlayed() || !MapImprints[i].UnitCtrlr.isInLadder)
            {
                CheckFoWVisibility(ctrlr, MapImprints[i]);
            }
        }
        if (MapImprint.maxFlag > 0)
        {
            MapImprint.currentFlagChecked = (MapImprint.currentFlagChecked + 1) % (MapImprint.maxFlag + 1);
        }
    }

    public void RefreshFoWTargetMoving(UnitController target)
    {
        if (target.Imprint.alwaysVisible)
        {
            return;
        }
        float @float = Constant.GetFloat(ConstantId.RANGE_SPELL_REQUIRED_PERC);
        WarbandController myWarbandCtrlr = GetMyWarbandCtrlr();
        for (int i = 0; i < myWarbandCtrlr.unitCtrlrs.Count; i++)
        {
            if (myWarbandCtrlr.unitCtrlrs[i].unit.Status != UnitStateId.OUT_OF_ACTION && myWarbandCtrlr.unitCtrlrs[i].isInLadder && target.IsInRange(myWarbandCtrlr.unitCtrlrs[i], 0f, target.unit.ViewDistance, @float, unitBlocking: false, checkAllBones: false, BoneId.NONE))
            {
                target.Imprint.AddViewer(myWarbandCtrlr.unitCtrlrs[i]);
            }
            else
            {
                target.Imprint.RemoveViewer(myWarbandCtrlr.unitCtrlrs[i]);
            }
        }
    }

    public void SetAccessibleActionZones(UnitController ctrlr, Action zonesSet)
    {
        StartCoroutine(CheckAllZonesAccessibility(ctrlr, zonesSet));
    }

    private IEnumerator CheckAllZonesAccessibility(UnitController ctrlr, Action zonesSet)
    {
        int counter = 0;
        float maxDist2 = ctrlr.unit.CurrentStrategyPoints * ctrlr.unit.Movement;
        maxDist2 *= maxDist2;
        accessibleActionZones.Clear();
        List<UnitController> allAliveUnits = GetAllAliveUnits();
        for (int j = 0; j < actionZones.Count; j++)
        {
            ActionZone zone = actionZones[j];
            if (!(zone != null))
            {
                continue;
            }
            if (Vector3.SqrMagnitude(ctrlr.transform.position - zone.transform.position) <= maxDist2)
            {
                zone.EnableFx(active: true);
                accessibleActionZones.Add(zone);
                zone.PointsChecker.UpdateControlPoints(ctrlr, allAliveUnits);
                counter++;
                if (counter >= 10)
                {
                    counter = 0;
                    yield return null;
                }
            }
            else
            {
                zone.EnableFx(active: false);
            }
        }
        for (int i = 0; i < triggerPoints.Count; i++)
        {
            Teleporter teleport = triggerPoints[i] as Teleporter;
            if (!(teleport != null) || !(Vector3.SqrMagnitude(ctrlr.transform.position - teleport.transform.position) <= maxDist2))
            {
                continue;
            }
            for (int pc = 0; pc < teleport.PointsCheckers.Count; pc++)
            {
                teleport.PointsCheckers[pc].UpdateControlPoints(ctrlr, allAliveUnits);
                counter++;
                if (counter >= 10)
                {
                    counter = 0;
                    yield return null;
                }
            }
        }
        zonesSet?.Invoke();
    }

    public void UnregisterDestructible(Destructible dest)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                WarbandCtrlrs[i].unitCtrlrs[j].triggeredDestructibles.Remove(dest);
            }
        }
        for (int num = triggerPoints.Count - 1; num >= 0; num--)
        {
            if (triggerPoints[num] == dest)
            {
                triggerPoints.RemoveAt(num);
            }
        }
    }

    public void RefreshActionZones(UnitController ctrlr)
    {
        float num = ctrlr.unit.Movement * 3;
        num *= num;
        for (int i = 0; i < accessibleActionZones.Count; i++)
        {
            ActionZone actionZone = accessibleActionZones[i];
            float num2 = Vector3.SqrMagnitude(ctrlr.transform.position - actionZone.transform.position);
            float num3 = Mathf.Min(num2, num) / num;
            actionZone.EnableFx(num2 < num);
        }
    }

    public void TurnOffActionZones()
    {
        for (int i = 0; i < actionZones.Count; i++)
        {
            ActionZone actionZone = actionZones[i];
            if (actionZone != null)
            {
                if (actionZone.destinations.Count > 0)
                {
                    actionZone.SetupFX();
                }
                actionZone.EnableFx(active: false);
            }
        }
    }

    public void MoveUnitsOnActionZone(UnitController currentUnit, PointsChecker pointsChecker, List<UnitController> units, bool isEnemy)
    {
        List<Vector3> validPoints = pointsChecker.validPoints;
        for (int i = 0; i < units.Count; i++)
        {
            float num = 9999f;
            Vector3 vector = Vector3.zero;
            for (int j = 0; j < validPoints.Count; j++)
            {
                float num2 = Vector3.SqrMagnitude(units[i].transform.position - validPoints[j]);
                if (num2 < num)
                {
                    num = num2;
                    vector = validPoints[j];
                }
            }
            validPoints.Remove(vector);
            if (isEnemy)
            {
                Vector3 value = vector - units[i].transform.position;
                vector += Vector3.Normalize(value) * -0.22f;
            }
            currentUnit.SendMoveAndUpdateCircle(units[i].uid, vector, units[i].transform.rotation);
        }
    }

    public void PlaySequence(string sequence, UnitController target, DelSequenceDone onFinishDel = null)
    {
        target.SetKinemantic(kine: true);
        focusedUnit = target;
        PandoraSingleton<SequenceManager>.Instance.PlaySequence(sequence, onFinishDel);
        TurnTimer.Pause();
    }

    public void ForceFocusedUnit(UnitController ctrlr)
    {
        focusedUnit = ctrlr;
    }

    public void SetBeaconLimit(int limit)
    {
        beaconLimit = limit;
    }

    public Beacon SpawnBeacon(Vector3 position)
    {
        Beacon beacon = null;
        for (int i = 0; i < beacons.Count; i++)
        {
            if (!beacons[i].gameObject.activeSelf)
            {
                beacon = beacons[i];
                beacon.gameObject.SetActive(value: true);
                break;
            }
        }
        if (beacon == null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(beaconPrefab);
            beacon = gameObject.GetComponent<Beacon>();
            beacons.Add(beacon);
        }
        beacon.transform.position = position;
        beacon.transform.rotation = Quaternion.identity;
        beacon.idx = ++beaconIdx;
        if (beaconLimit != 0)
        {
            lastBeaconIdx = beaconIdx - beaconLimit;
        }
        RefreshBeacons();
        return beacon;
    }

    public void RevertBeacons(Beacon keavin)
    {
        beaconIdx = keavin.idx;
        RefreshBeacons();
    }

    public int ActiveBeacons()
    {
        return beaconIdx;
    }

    public void ClearBeacons()
    {
        for (int num = beacons.Count - 1; num >= 0; num--)
        {
            if (beacons[num] != null && beacons[num].gameObject != null)
            {
                beacons[num].gameObject.SetActive(value: false);
            }
        }
        beaconIdx = 0;
        lastBeaconIdx = 0;
        RefreshBeacons();
    }

    private void RefreshBeacons()
    {
        for (int i = 0; i < beacons.Count; i++)
        {
            beacons[i].gameObject.SetActive(beacons[i].idx > lastBeaconIdx && beacons[i].idx <= beaconIdx);
        }
    }

    private void InitBeacons()
    {
        mapBeacons = new List<MapBeacon>();
        int i;
        for (i = 0; i < 5; i++)
        {
            PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/beacon/", AssetBundleId.FX, "map_beacon_" + i + ".prefab", delegate (UnityEngine.Object prefab)
            {
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
                MapBeacon component = gameObject.GetComponent<MapBeacon>();
                mapBeacons.Add(component);
                gameObject.SetActive(value: false);
            });
        }
        PandoraSingleton<AssetBundleLoader>.instance.LoadAssetAsync<GameObject>("Assets/prefabs/beacon/", AssetBundleId.FX, "beacon.prefab", delegate (UnityEngine.Object prefab)
        {
            beaconPrefab = (GameObject)prefab;
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_unit_placement_blue_01.prefab", delegate (UnityEngine.Object prefab)
        {
            deployBeaconPrefab = (GameObject)prefab;
        });
    }

    public List<MapBeacon> GetMapBeacons()
    {
        return mapBeacons;
    }

    private MapBeacon GetFirstAvailableMapBeacon()
    {
        for (int i = 0; i < mapBeacons.Count; i++)
        {
            if (!mapBeacons[i].gameObject.activeSelf)
            {
                return mapBeacons[i];
            }
        }
        return null;
    }

    public MapBeacon SpawnMapBeacon(Vector3 pos, Action spawnCb)
    {
        MapBeacon firstAvailableMapBeacon = GetFirstAvailableMapBeacon();
        if (firstAvailableMapBeacon != null)
        {
            firstAvailableMapBeacon.transform.position = pos;
            firstAvailableMapBeacon.transform.rotation = Quaternion.identity;
            StartCoroutine(ActivateBeaconNextFrame(firstAvailableMapBeacon, spawnCb));
            return firstAvailableMapBeacon;
        }
        return null;
    }

    private IEnumerator ActivateBeaconNextFrame(MapBeacon beacon, Action spawnCb)
    {
        yield return false;
        beacon.gameObject.SetActive(value: true);
        spawnCb?.Invoke();
    }

    public void RemoveMapBecon(MapBeacon mapBeacon)
    {
        mapBeacon.gameObject.SetActive(value: false);
    }

    public int GetAvailableMapBeacons()
    {
        int num = 0;
        for (int i = 0; i < mapBeacons.Count; i++)
        {
            if (mapBeacons[i].gameObject.activeSelf)
            {
                num++;
            }
        }
        return num;
    }

    public void ActivateMapObjectiveZones(bool activate)
    {
        List<Objective> objectives = GetMyWarbandCtrlr().objectives;
        for (int i = 0; i < mapObjectiveZones.Count; i++)
        {
            bool flag = activate;
            if (flag && mapObjectiveZones[i].objectiveId != 0)
            {
                for (int j = 0; j < objectives.Count; j++)
                {
                    if (objectives[j].Id == mapObjectiveZones[i].objectiveId)
                    {
                        flag = (!objectives[j].done && !objectives[j].Locked);
                    }
                }
            }
            mapObjectiveZones[i].gameObject.SetActive(flag);
        }
    }

    private void InitTargetingAssets()
    {
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_zone_target_attack_aoe_01.prefab", delegate (UnityEngine.Object go)
        {
            aoeTargetSphere = (GameObject)UnityEngine.Object.Instantiate(go);
            aoeTargetSphere.SetActive(value: false);
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_zone_top_attack_aoe_01.prefab", delegate (UnityEngine.Object go)
        {
            aoeGroundTargetSphere = (GameObject)UnityEngine.Object.Instantiate(go);
            aoeGroundTargetSphere.SetActive(value: false);
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_aoe_cone_01.prefab", delegate (UnityEngine.Object go)
        {
            coneTarget = (GameObject)UnityEngine.Object.Instantiate(go);
            coneTarget.SetActive(value: false);
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_aoe_arc_01.prefab", delegate (UnityEngine.Object go)
        {
            arcTarget = (GameObject)UnityEngine.Object.Instantiate(go);
            arcTarget.SetActive(value: false);
        });
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_aoe_cylindre_01.prefab", delegate (UnityEngine.Object go)
        {
            lineTarget = (GameObject)UnityEngine.Object.Instantiate(go);
            lineTarget.SetActive(value: false);
        });
    }

    public void InitSphereTarget(Transform src, float radius, TargetingId targetingId, out Vector3 sphereRaySrc, out Vector3 sphereDir)
    {
        if (sphereTarget != null)
        {
            sphereTarget.SetActive(value: false);
        }
        sphereRaySrc = Vector3.zero;
        sphereDir = Vector3.zero;
        switch (targetingId)
        {
            case TargetingId.AREA:
                sphereTarget = aoeTargetSphere;
                sphereRaySrc = src.position + Vector3.up * 1.25f;
                sphereDir = Vector3.Normalize(src.forward + src.up * -0.5f);
                break;
            case TargetingId.AREA_GROUND:
                sphereTarget = aoeGroundTargetSphere;
                sphereRaySrc = src.position + Vector3.up * 1.25f / 2f;
                sphereDir = src.forward;
                break;
            default:
                PandoraDebug.LogError("Targeting " + targetingId + " is not supported in InitSphereTargeting");
                break;
        }
        sphereTarget.SetActive(value: false);
        sphereTarget.transform.localScale = Vector3.one * radius * 2f;
        sphereTarget.transform.position = sphereRaySrc + sphereDir;
    }

    public void InitConeTarget(Transform src, float radius, float range, out Vector3 coneSrc, out Vector3 coneDir)
    {
        coneSrc = src.position + Vector3.up;
        coneDir = Vector3.Normalize(src.forward);
        coneTarget.transform.position = coneSrc;
        Quaternion rotation = Quaternion.LookRotation(coneDir);
        coneTarget.transform.rotation = rotation;
        coneTarget.transform.GetChild(0).localScale = new Vector3(radius * 2f, range, radius * 2f);
    }

    public void InitArcTarget(Transform src, out Vector3 arcSrc, out Vector3 arcDir)
    {
        arcSrc = src.position + Vector3.up;
        arcTarget.transform.position = arcSrc;
        arcDir = src.transform.forward;
        arcTarget.transform.rotation = Quaternion.LookRotation(arcDir);
    }

    public void InitLineTarget(Transform src, float radius, float range, out Vector3 lineSrc, out Vector3 lineDir)
    {
        lineSrc = src.position + Vector3.up;
        lineDir = Vector3.Normalize(src.forward);
        lineTarget.transform.position = lineSrc;
        Quaternion rotation = Quaternion.LookRotation(lineDir);
        lineTarget.transform.rotation = rotation;
        lineTarget.transform.GetChild(0).localScale = new Vector3(radius * 2f, range, radius * 2f);
    }

    public UnitController ActivateHiddenUnit(CampaignUnitId campaignUnitId, bool spawnVisible, string loc = "mission_unit_spawn")
    {
        UnitController unitController = excludedUnits.Find((UnitController x) => x.unit.CampaignData.Id == campaignUnitId);
        if (unitController != null)
        {
            UnitController currentUnit = GetCurrentUnit();
            currentUnit = ((!(currentUnit == null)) ? currentUnit : unitController);
            List<DecisionPoint> availableSpawnPoints = GetAvailableSpawnPoints(spawnVisible, asc: true, currentUnit.transform, unitController.forcedSpawnPoints);
            if (availableSpawnPoints.Count > 0)
            {
                IncludeUnit(unitController, availableSpawnPoints[0].transform.position, availableSpawnPoints[0].transform.rotation);
                ForceUnitVisibilityCheck(unitController);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_UNIT_SPAWN, loc, unitController.unit.Name);
            }
            else
            {
                PandoraDebug.LogWarning("Trying to spawn " + campaignUnitId + " but there is no spawn points available", "INTERACTIVE POINT");
            }
        }
        else
        {
            PandoraDebug.LogWarning("Trying to spawn campaign unit " + campaignUnitId + " but it's not in the hiddent list", "INTERACTIVE POINT");
        }
        return unitController;
    }

    public void SetCombatCircles(UnitController ctrlr, Action onCirclesSet)
    {
        StartCoroutine(SetCombatAllCircles(ctrlr, onCirclesSet));
    }

    private IEnumerator SetCombatAllCircles(UnitController ctrlr, Action onCirclesSet)
    {
        lockNavRefresh = true;
        for (int j = 0; j < WarbandCtrlrs.Count; j++)
        {
            for (int i = 0; i < WarbandCtrlrs[j].unitCtrlrs.Count; i++)
            {
                WarbandCtrlrs[j].unitCtrlrs[i].SetCombatCircle(ctrlr);
                yield return null;
            }
        }
        lockNavRefresh = false;
        onCirclesSet?.Invoke();
    }

    public void UpdateCombatCirclesAlpha(UnitController ctrlr)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                WarbandCtrlrs[i].unitCtrlrs[j].SetCombatCircleAlpha(ctrlr);
            }
        }
    }

    public void ShowCombatCircles(UnitController currentCtrlr)
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                UnitController unitController = WarbandCtrlrs[i].unitCtrlrs[j];
                if (unitController != this && unitController.unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    unitController.combatCircle.Show(visible: true);
                    unitController.combatCircle.SetMaterial(currentCtrlr.IsEnemy(unitController), unitController.Engaged);
                }
            }
        }
        UpdateCombatCirclesAlpha(currentCtrlr);
    }

    public void HideCombatCircles()
    {
        for (int i = 0; i < WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                WarbandCtrlrs[i].unitCtrlrs[j].combatCircle.Show(visible: false);
            }
        }
    }
}
