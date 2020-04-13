using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideoutManager : PandoraSingleton<HideoutManager>
{
    public enum State
    {
        CAMP,
        WARBAND,
        INVENTORY,
        SKIRMISH,
        LOBBY,
        END_GAME,
        END_GAME_WARBAND,
        SKILLS,
        SPELLS,
        SHIPMENT,
        SHOP,
        PLAYER_PROGRESSION,
        MISSION,
        MISSION_PREPARATION,
        HIRE,
        UNIT_INFO,
        CUSTOMIZATION,
        OPTIONS,
        WARBAND_STATS,
        COUNT
    }

    public enum NextDayOrder
    {
        NONE,
        NEXT_DAY,
        NEW_MISSION,
        SHIPMENT_LATE,
        MISSION_REWARDS
    }

    public enum HideoutTutoType
    {
        NONE = 0,
        CAMP = 1,
        MANAGEMENT = 2,
        SMUGGLER = 4,
        PROGRESSION = 8,
        SKIRMISH = 0x10,
        CAMPAIGN = 0x20,
        SHOP = 0x40,
        UNIT = 0x80,
        HIRE = 0x100,
        CAMP_2 = 0x200,
        CAMP_3 = 0x400
    }

    public List<Mission> missions;

    private List<KeyValuePair<int, int>> usedPositions;

    public MenuNodeGroup campNodeGroup;

    public MenuNode warbandNodeWagon;

    public MenuNode warbandNodeFlag;

    public MenuNodeGroup warbandNodeGroup;

    public MenuNodeGroup skirmishNodeGroup;

    public MenuNode shopNode;

    public MenuNode progressNode;

    public MenuNode mapNode;

    public MenuNode unitNode;

    public GameObject optionsPanel;

    public GameObject shopNodePrefab;

    public GameObject shipmentNodePrefab;

    public GameObject nextDayNodePrefab;

    private GameObject shipmentNodeContent;

    private GameObject shopNodeContent;

    private GameObject nextDayNodeContent;

    public ConfirmationPopupView messagePopup;

    public DailyReportPopup nextDayPopup;

    public NewMissionPopup newMissionPopup;

    public TextInputPopup textInputPopup;

    public MissionRewardPopup missionRewardPopup;

    public ShopConfirmPopup shopConfirmPopup;

    [HideInInspector]
    public UnitMenuController currentUnit;

    [HideInInspector]
    public int currentWarbandSlotIdx;

    [HideInInspector]
    public bool currentWarbandSlotHireImpressive;

    private NextDayOrder popupOrder;

    private int currentMissionRewardsIndex;

    public bool finishedLoading;

    public bool startedLoading;

    public bool transitionDone;

    private HideoutTutoType nextTuto;

    [HideInInspector]
    public bool showingTuto;

    private bool checkInvite;

    [HideInInspector]
    public bool checkPlayTogether;

    public PlayTogetherPopupView playTogetherPopup;

    public int tabsLoading;

    public CheapStateMachine StateMachine
    {
        get;
        private set;
    }

    public WarbandMenuController WarbandCtrlr
    {
        get;
        private set;
    }

    public WarbandChest WarbandChest
    {
        get;
        private set;
    }

    public Market Market
    {
        get;
        private set;
    }

    public Progressor Progressor
    {
        get;
        private set;
    }

    public Date Date
    {
        get;
        private set;
    }

    public CameraManager CamManager
    {
        get;
        private set;
    }

    public bool IsTrainingOutsider
    {
        get;
        set;
    }

    private void Awake()
    {
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
        HideoutCamAnchor[] componentsInChildren = GetComponentsInChildren<HideoutCamAnchor>();
        StateMachine = new CheapStateMachine(19);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            switch (componentsInChildren[i].state)
            {
                case State.CAMP:
                    StateMachine.AddState(new HideoutCamp(this, componentsInChildren[i]), 0);
                    break;
                case State.SKIRMISH:
                    StateMachine.AddState(new HideoutSkirmish(this, componentsInChildren[i]), 3);
                    break;
                case State.WARBAND:
                    StateMachine.AddState(new HideoutWarband(this, componentsInChildren[i]), 1);
                    break;
                case State.INVENTORY:
                    StateMachine.AddState(new HideoutInventory(this, componentsInChildren[i]), 2);
                    break;
                case State.SKILLS:
                    StateMachine.AddState(new HideoutSkills(this, componentsInChildren[i], showSpell: false), 7);
                    break;
                case State.SPELLS:
                    StateMachine.AddState(new HideoutSkills(this, componentsInChildren[i], showSpell: true), 8);
                    break;
                case State.CUSTOMIZATION:
                    StateMachine.AddState(new HideoutCustomization(this, componentsInChildren[i]), 16);
                    break;
                case State.LOBBY:
                    StateMachine.AddState(new HideoutLobby(this, componentsInChildren[i]), 4);
                    break;
                case State.END_GAME:
                    StateMachine.AddState(new HideoutEndGame(this, componentsInChildren[i]), 5);
                    break;
                case State.END_GAME_WARBAND:
                    StateMachine.AddState(new HideoutEndGameWarband(this, componentsInChildren[i]), 6);
                    break;
                case State.SHIPMENT:
                    StateMachine.AddState(new HideoutSmuggler(this, componentsInChildren[i]), 9);
                    break;
                case State.MISSION:
                    StateMachine.AddState(new HideoutMission(this, componentsInChildren[i]), 12);
                    break;
                case State.MISSION_PREPARATION:
                    StateMachine.AddState(new HideoutMissionPrep(this, componentsInChildren[i]), 13);
                    break;
                case State.HIRE:
                    StateMachine.AddState(new HideoutHire(this, componentsInChildren[i]), 14);
                    break;
                case State.SHOP:
                    StateMachine.AddState(new HideoutShop(this, componentsInChildren[i]), 10);
                    break;
                case State.UNIT_INFO:
                    StateMachine.AddState(new HideoutUnitInfo(this, componentsInChildren[i]), 15);
                    break;
                case State.PLAYER_PROGRESSION:
                    StateMachine.AddState(new HideoutPlayerProgression(this, componentsInChildren[i]), 11);
                    break;
                case State.WARBAND_STATS:
                    StateMachine.AddState(new HideoutWarbandStats(this, componentsInChildren[i]), 18);
                    break;
            }
        }
        StateMachine.AddState(new HideoutOptions(this), 17);
        if (!PandoraSingleton<MissionStartData>.Exists())
        {
            GameObject gameObject = new GameObject("mission_start_data");
            gameObject.AddComponent<MissionStartData>();
        }
        else
        {
            PandoraSingleton<MissionStartData>.Instance.Clear();
        }
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_INVITE_ACCEPTED, InviteReceived);
        StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        yield return null;
        yield return StartCoroutine(PandoraSingleton<HideoutTabManager>.Instance.Load());
        yield return null;
        Progressor = new Progressor();
        yield return null;
        WarbandSave warSave = PandoraSingleton<GameManager>.instance.currentSave;
        Date = new Date(warSave.currentDate);
        WarbandCtrlr = new WarbandMenuController(warSave);
        yield return null;
        WarbandChest = new WarbandChest(WarbandCtrlr.Warband);
        yield return null;
        Market = new Market(WarbandCtrlr.Warband);
        yield return null;
        GetShopNodeContent();
        yield return null;
        CamManager = Camera.main.GetComponent<CameraManager>();
        CameraSetter cameraSetter = GetComponent<CameraSetter>();
        cameraSetter.SetCameraInfo(Camera.main);
        Camera.main.enabled = false;
        bool wasSkirmish = false;
        if (warSave.inMission)
        {
            wasSkirmish = warSave.endMission.isSkirmish;
        }
        if (!WarbandCtrlr.Warband.GetWarbandSave().inMission || wasSkirmish)
        {
            GenerateMissions(newday: true);
        }
        else
        {
            missions = new List<Mission>();
            warSave.missions.Clear();
            warSave.scoutsSent = -1;
        }
        PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
        yield return null;
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TRANSITION_DONE, OnTransitionDone);
        ValidateDLCs();
        ValidateDeadUnits();
        ValidateNewMutations();
        ValidateOutsiderRotationMissingEvent();
        startedLoading = true;
        yield return StartCoroutine(WarbandCtrlr.GenerateUnits());
        WarbandCtrlr.SetBannerWagon();
        yield return null;
        WarbandCtrlr.GenerateMap();
        startedLoading = false;
        yield return null;
        WarbandCtrlr.Warband.CheckRespecPoints();
        PandoraSingleton<GameManager>.Instance.Profile.CheckXp();
        PandoraSingleton<Hephaestus>.Instance.SetDLCBoughtCb(ValidateDLCs);
        PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
        PandoraSingleton<Hermes>.Instance.StopConnections();
        SaveChanges();
        yield return null;
        WarbandCtrlr.Warband.UpdateAttributes();
        string music = "hideout_" + WarbandCtrlr.Warband.WarbandData.Id.ToLowerString();
        PandoraSingleton<Pan>.Instance.PlayMusic(music, ambiance: false);
        PandoraSingleton<Hephaestus>.Instance.SetRichPresence(Hephaestus.RichPresenceId.HIDEOUT, active: true);
        while (!finishedLoading)
        {
            yield return null;
        }
        CamManager.gameObject.GetComponent<Camera>().enabled = true;
        while (tabsLoading > 0)
        {
            yield return null;
        }
        yield return StartCoroutine(PandoraSingleton<HideoutTabManager>.Instance.ParentModules());
        PandoraDebug.LogDebug("Loading Finished!");
        PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone();
        if (WarbandCtrlr.Warband.GetWarbandSave().inMission)
        {
            WarbandCtrlr.Warband.GetWarbandSave().inMission = false;
            StateMachine.ChangeState(5);
            yield break;
        }
        if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            checkInvite = true;
        }
        if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
        {
            checkPlayTogether = true;
        }
        StateMachine.ChangeState(0);
    }

    private void ValidateDeadUnits()
    {
        for (int i = 0; i < WarbandCtrlr.Units.Count; i++)
        {
            Unit unit = WarbandCtrlr.Units[i];
            for (int j = 0; j < unit.Injuries.Count; j++)
            {
                if (unit.IsInjuryAttributeLimitExceeded(unit.Injuries[j].Data, checkRetire: true) || unit.IsInjuryRepeatLimitExceeded(unit.Injuries[j].Data, post: true))
                {
                    WarbandCtrlr.Disband(unit, (unit.Injuries[j].Data.Id != InjuryId.DEAD) ? EventLogger.LogEvent.RETIREMENT : EventLogger.LogEvent.DEATH, (int)unit.Injuries[j].Data.Id);
                    break;
                }
            }
        }
    }

    private void ValidateNewMutations()
    {
        for (int i = 0; i < WarbandCtrlr.Units.Count; i++)
        {
            CheckForMutation(WarbandCtrlr.Units[i], outsider: false);
        }
        for (int j = 0; j < WarbandCtrlr.Warband.Outsiders.Count; j++)
        {
            CheckForMutation(WarbandCtrlr.Warband.Outsiders[j], outsider: true);
        }
    }

    private void CheckForMutation(Unit unit, bool outsider)
    {
        if (unit.UnitSave.mutationChecked)
        {
            return;
        }
        if (unit.Rank == 4 || unit.Rank == 7 || unit.Rank == 8 || unit.Rank == 9)
        {
            List<UnitJoinUnitRankData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinUnitRankData>(new string[2]
            {
                "fk_unit_id",
                "mutation"
            }, new string[2]
            {
                unit.Id.ToIntString(),
                "1"
            });
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                int num2 = (int)((float)list[i].UnitRankId / 10f);
                if (unit.Rank >= num2)
                {
                    num++;
                }
            }
            while (unit.Mutations.Count < num)
            {
                List<Item> list2 = new List<Item>();
                Mutation mutation = unit.AddRandomMutation(list2);
                unit.ResetBodyPart();
                if (!outsider)
                {
                    WarbandChest.AddItems(list2);
                }
            }
        }
        unit.UnitSave.mutationChecked = true;
    }

    private void ValidateOutsiderRotationMissingEvent()
    {
        if (WarbandCtrlr.Warband.GetAttribute(WarbandAttributeId.OUTSIDERS_COUNT) > 0 && WarbandCtrlr.Warband.Logger.FindEventsAfter(EventLogger.LogEvent.OUTSIDER_ROTATION, Date.CurrentDate + 1).Count < 2)
        {
            WarbandCtrlr.Warband.AddOutsiderRotationEvent();
        }
    }

    private void ValidateDLCs()
    {
        CheckUnitDLC(Hephaestus.DlcId.SMUGGLER, AllegianceId.ORDER, WarbandSkillId.DLC_SMUGGLER);
        CheckUnitDLC(Hephaestus.DlcId.PRIEST_OF_ULRIC, AllegianceId.ORDER, WarbandSkillId.DLC_PRIEST_OF_ULRIC);
        CheckUnitDLC(Hephaestus.DlcId.GLOBADIER, AllegianceId.DESTRUCTION, WarbandSkillId.DLC_GLOBADIER);
        CheckUnitDLC(Hephaestus.DlcId.DOOMWEAVER, AllegianceId.DESTRUCTION, WarbandSkillId.DLC_DOOMWEAVER);
    }

    private void CheckUnitDLC(Hephaestus.DlcId dlcId, AllegianceId requestedAllegiance, WarbandSkillId grantedSkillId)
    {
        if (PandoraSingleton<Hephaestus>.Instance.OwnsDLC(dlcId) && WarbandCtrlr.Warband.WarbandData.AllegianceId == requestedAllegiance && WarbandCtrlr.Warband.GetWarbandSave().skills.IndexOf(grantedSkillId, WarbandSkillIdComparer.Instance) == -1)
        {
            WarbandCtrlr.Warband.LearnSkill(new WarbandSkill(grantedSkillId));
        }
    }

    public bool IsPostMission()
    {
        return WarbandCtrlr.Warband.GetWarbandSave().scoutsSent < 0;
    }

    public UnitMenuController GetUnitMenuController(Unit unit)
    {
        for (int i = 0; i < WarbandCtrlr.unitCtrlrs.Count; i++)
        {
            if (WarbandCtrlr.unitCtrlrs[i].unit == unit)
            {
                return WarbandCtrlr.unitCtrlrs[i];
            }
        }
        return null;
    }

    public void GenerateMissions(bool newday)
    {
        missions = new List<Mission>();
        if (newday && WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx <= Constant.GetInt(ConstantId.CAMPAIGN_LAST_MISSION))
        {
            Tuple<int, EventLogger.LogEvent, int> tuple = WarbandCtrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.NEW_MISSION);
            if (tuple.Item1 <= Date.CurrentDate && tuple.Item3 == WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx)
            {
                missions.Add(Mission.GenerateCampaignMission(WarbandCtrlr.Warband.Id, WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx));
            }
        }
        usedPositions = new List<KeyValuePair<int, int>>();
        if (WarbandCtrlr.Warband.GetWarbandSave().missions.Count == 0)
        {
            int attribute = WarbandCtrlr.Warband.GetAttribute(WarbandAttributeId.PROC_MISSIONS_AVAILABLE);
            for (int i = 0; i < attribute; i++)
            {
                AddProcMission(i != 0);
            }
            SaveChanges();
            return;
        }
        WarbandSave warbandSave = WarbandCtrlr.Warband.GetWarbandSave();
        for (int j = 0; j < warbandSave.missions.Count; j++)
        {
            Mission mission = new Mission(warbandSave.missions[j]);
            missions.Add(mission);
            usedPositions.Add(new KeyValuePair<int, int>((int)mission.GetDistrictId(), mission.missionSave.mapPosition));
        }
    }

    public Mission AddProcMission(bool boost)
    {
        Mission mission = Mission.GenerateProcMission(ref usedPositions, WarbandCtrlr.Warband.WyrdstoneDensityModifiers, WarbandCtrlr.Warband.SearchDensityModifiers, WarbandCtrlr.Warband.MissionRatingModifiers);
        WarbandCtrlr.Warband.GetWarbandSave().missions.Add(mission.missionSave);
        missions.Add(mission);
        return mission;
    }

    private void OnTransitionDone()
    {
        PandoraDebug.LogInfo("OnTransitionDone", "FLOW", this);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_DONE, OnTransitionDone);
        transitionDone = true;
        if (nextTuto != 0)
        {
            ShowHideoutTuto(nextTuto);
            nextTuto = HideoutTutoType.NONE;
        }
    }

    private void OnDestroy()
    {
        StateMachine.Destroy();
        PandoraSingleton<Hephaestus>.Instance.SetDLCBoughtCb(null);
    }

    public void OnNextDay()
    {
        if (finishedLoading)
        {
            popupOrder = NextDayOrder.NONE;
            currentMissionRewardsIndex = 0;
            ShowNextPopup();
        }
    }

    private void ShowNextPopup()
    {
        popupOrder++;
        Tuple<int, EventLogger.LogEvent, int> tuple = null;
        switch (popupOrder)
        {
            case NextDayOrder.NONE:
                break;
            case NextDayOrder.NEXT_DAY:
                messagePopup.Show("hideout_day_skip", "hideout_day_skip_desc", OnNextDayPopup);
                break;
            case NextDayOrder.NEW_MISSION:
                if (WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx <= Constant.GetInt(ConstantId.CAMPAIGN_LAST_MISSION))
                {
                    tuple = WarbandCtrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.NEW_MISSION);
                }
                if (tuple != null && tuple.Item1 == Date.CurrentDate && tuple.Item3 == WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx)
                {
                    newMissionPopup.Setup(OnNewMissionConfirm);
                    PandoraSingleton<Pan>.Instance.Narrate("new_campaign_mission" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 3));
                }
                else
                {
                    ShowNextPopup();
                }
                break;
            case NextDayOrder.SHIPMENT_LATE:
                tuple = WarbandCtrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
                if (tuple != null && tuple.Item1 + 1 == Date.CurrentDate)
                {
                    WarbandSave warbandSave = WarbandCtrlr.Warband.GetWarbandSave();
                    Unit unit = Progressor.CheckLateShipment(warbandSave, WarbandCtrlr, WarbandCtrlr.PrimaryFactionController);
                    ((HideoutCamp)StateMachine.GetActiveState()).RefreshButtons();
                    PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.TALL, PopupModuleId.POPUP_GENERIC_ANCHOR);
                    List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[1]
                    {
                    PopupModuleId.POPUP_GENERIC_ANCHOR
                    });
                    ConfirmationPopupView confirmationPopupView = (ConfirmationPopupView)modulesPopup[0];
                    if (unit != null)
                    {
                        confirmationPopupView.ShowLocalized(PandoraSingleton<LocalizationManager>.instance.GetStringById("popup_shipment_late_title"), PandoraSingleton<LocalizationManager>.instance.GetStringById(WarbandCtrlr.PrimaryFactionController.GetConsequenceLabel(), unit.Name), OnShipmentLatePopup);
                        confirmationPopupView.HideCancelButton();
                    }
                    else
                    {
                        confirmationPopupView.ShowLocalized(PandoraSingleton<LocalizationManager>.instance.GetStringById("popup_shipment_late_title"), PandoraSingleton<LocalizationManager>.instance.GetStringById(WarbandCtrlr.PrimaryFactionController.GetConsequenceLabel() + "_no_units"), OnShipmentLatePopup);
                        confirmationPopupView.HideCancelButton();
                    }
                    PandoraSingleton<Pan>.Instance.Narrate("shipment_failed" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 3));
                    SaveChanges();
                }
                else
                {
                    ShowNextPopup();
                }
                break;
            case NextDayOrder.MISSION_REWARDS:
                {
                    List<Tuple<int, EventLogger.LogEvent, int>> list = WarbandCtrlr.Warband.Logger.FindEventsAtDay(EventLogger.LogEvent.MISSION_REWARDS, Date.CurrentDate);
                    if (list.Count > 0 && currentMissionRewardsIndex < list.Count)
                    {
                        tuple = list[currentMissionRewardsIndex];
                        currentMissionRewardsIndex++;
                        List<CampaignMissionData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(new string[2]
                        {
                    "fk_warband_id",
                    "idx"
                        }, new string[2]
                        {
                    ((int)WarbandCtrlr.Warband.WarbandData.Id).ToString(),
                    tuple.Item3.ToString()
                        });
                        WarbandSkillId warbandSkillIdReward = list2[0].WarbandSkillIdReward;
                        WarbandCtrlr.Warband.AddSkill(warbandSkillIdReward, isNew: true);
                        SaveChanges();
                        List<WarbandSkillItemData> itemRewards = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillItemData>("fk_warband_skill_id", warbandSkillIdReward.ToIntString());
                        List<WarbandSkillFreeOutsiderData> freeOutsiders = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillFreeOutsiderData>("fk_warband_skill_id", warbandSkillIdReward.ToIntString());
                        missionRewardPopup.Show(OnMissionRewardPopup, itemRewards, freeOutsiders);
                        WarbandCtrlr.GenerateHireableUnits();
                        RefreshTreasury();
                        PandoraSingleton<Pan>.Instance.Narrate("shipment_completed");
                    }
                    else
                    {
                        ShowNextPopup();
                    }
                    break;
                }
        }
    }

    private void OnMissionRewardPopup(bool isConfirm)
    {
        popupOrder--;
        ShowNextPopup();
    }

    private void OnShipmentLatePopup(bool isConfirm)
    {
        ShowNextPopup();
    }

    private void OnNextDayPopup(bool isConfirm)
    {
        if (isConfirm)
        {
            Date.NextDay();
            WarbandCtrlr.Warband.GetWarbandSave().currentDate = Date.CurrentDate;
            PandoraDebug.LogInfo("New current date " + Date.CurrentDate, "MENUS");
            if (Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) + Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR))
            {
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.YEAR_1);
            }
            else if (Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) + Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR) * 5)
            {
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.YEAR_5);
            }
            List<UnitMenuController> list = new List<UnitMenuController>(WarbandCtrlr.unitCtrlrs);
            for (int i = 0; i < list.Count; i++)
            {
                Progressor.NextDayUnitProgress(list[i].unit);
            }
            Progressor.NextDayWarbandProgress();
            SaveChanges();
            nextDayPopup.SetUnitList(list);
            nextDayPopup.Show(OnNextDayFinished);
            RefreshTreasury();
            HideoutCamp hideoutCamp = (HideoutCamp)StateMachine.GetActiveState();
            hideoutCamp.RefreshButtons();
        }
    }

    private void RefreshTreasury()
    {
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(WarbandCtrlr.Warband.GetWarbandSave());
    }

    private void OnNextDayFinished(bool isConfirm)
    {
        nextDayPopup.SetUnitList(null);
        PandoraDebug.LogDebug("OnNextDayFinished!");
        ShowNextPopup();
    }

    private void OnNewMissionConfirm(bool isConfirm)
    {
        ShowNextPopup();
    }

    private void Update()
    {
        if (transitionDone)
        {
            if (checkInvite)
            {
                CheckInvite();
            }
            else if (checkPlayTogether)
            {
                CheckPlayTogether();
            }
        }
        StateMachine.Update();
        if (WarbandCtrlr != null)
        {
            WarbandCtrlr.generatingHireable = false;
        }
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    public void SaveChanges()
    {
        if (WarbandCtrlr != null)
        {
            PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(WarbandCtrlr.Warband.GetWarbandSave(), PandoraSingleton<GameManager>.Instance.campaign);
        }
    }

    public GameObject GetShipmentNodeContent()
    {
        if (shipmentNodeContent == null)
        {
            shipmentNodeContent = Object.Instantiate(shipmentNodePrefab);
        }
        return shipmentNodeContent;
    }

    public GameObject GetNextDayNodeContent()
    {
        if (nextDayNodeContent == null)
        {
            nextDayNodeContent = Object.Instantiate(nextDayNodePrefab);
        }
        return nextDayNodeContent;
    }

    public GameObject GetShopNodeContent()
    {
        if (shopNodeContent == null)
        {
            shopNodeContent = Object.Instantiate(shopNodePrefab);
            shopNodeContent.GetComponentInChildren<RackLoadoutNode>().SetLoadout(WarbandCtrlr.Warband.WarbandData.Wagon);
        }
        return shopNodeContent;
    }

    public void ShowHideoutTuto(HideoutTutoType type)
    {
        if (!PandoraSingleton<GameManager>.Instance.Options.skipTuto)
        {
            if (!transitionDone)
            {
                nextTuto = type;
            }
            else
            {
                StartCoroutine(ShowTuto(type));
            }
        }
    }

    private IEnumerator ShowTuto(HideoutTutoType type)
    {
        if (!WarbandCtrlr.Warband.HasShownHideoutTuto(type) && !PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            showingTuto = true;
            yield return new WaitForSeconds(0.2f);
            PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.TALL, PopupModuleId.POPUP_GENERIC_ANCHOR);
            List<UIPopupModule> modules = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[1]
            {
                PopupModuleId.POPUP_GENERIC_ANCHOR
            });
            PandoraDebug.LogDebug("module length = " + modules.Count);
            ConfirmationPopupView popup = modules[0].GetComponent<ConfirmationPopupView>();
            string typeTitle = type.ToString();
            string typeDesc = type.ToString();
            if (typeTitle[typeTitle.Length - 2] == '_')
            {
                typeTitle = typeTitle.Substring(0, typeTitle.Length - 2);
            }
            WarbandCtrlr.Warband.SetHideoutTutoShown(type);
            SaveChanges();
            popup.Show("hideout_" + typeTitle, "hideout_tuto_" + typeDesc, OnTutoMessageClose);
            popup.HideCancelButton();
        }
    }

    private void OnTutoMessageClose(bool confirm)
    {
        showingTuto = false;
    }

    private void InviteReceived()
    {
        checkInvite = true;
    }

    public bool IsCheckingInvite()
    {
        return checkInvite || checkPlayTogether;
    }

    private void CheckInvite()
    {
        checkInvite = false;
        if (!WarbandCtrlr.Warband.ValidateWarbandForInvite(inMission: false))
        {
            PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_hideout_quit_title", "invite_hideout_quit_desc", OnHideoutReceiveInviteShouldSaveAndQuit, showCancel: true);
        }
        else if (StateMachine.GetActiveStateId() == 4)
        {
            PandoraSingleton<SkirmishManager>.Instance.LeaveLobby();
        }
        else if (StateMachine.GetActiveStateId() != 3 && StateMachine.GetActiveStateId() != 5 && StateMachine.GetActiveStateId() != 6 && StateMachine.GetActiveStateId() != 4)
        {
            StateMachine.ChangeState(3);
        }
        else
        {
            checkInvite = (StateMachine.GetActiveStateId() != 3);
        }
    }

    private void CheckPlayTogether()
    {
        checkPlayTogether = false;
        if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
        {
            if (!WarbandCtrlr.Warband.IsSkirmishAvailable(out string reason) && !WarbandCtrlr.Warband.IsContestAvailable(out reason))
            {
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "play_together_hideout_quit_title", "play_together_hideout_quit_desc", OnHideoutReceiveInviteShouldSaveAndQuit, showCancel: true);
            }
            else if (StateMachine.GetActiveStateId() == 4)
            {
                PandoraSingleton<SkirmishManager>.Instance.LeaveLobby();
            }
            else if (StateMachine.GetActiveStateId() != 3 && StateMachine.GetActiveStateId() != 5 && StateMachine.GetActiveStateId() != 6 && StateMachine.GetActiveStateId() != 4)
            {
                StateMachine.ChangeState(3);
            }
        }
    }

    private void OnHideoutReceiveInviteShouldSaveAndQuit(bool confirm)
    {
        if (confirm)
        {
            SaveChanges();
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME);
        }
        else
        {
            PandoraSingleton<Hephaestus>.Instance.ResetInvite();
            PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: true);
        }
    }
}
