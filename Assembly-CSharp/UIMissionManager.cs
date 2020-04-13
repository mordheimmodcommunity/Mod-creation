using System.Collections.Generic;
using UnityEngine;

public class UIMissionManager : PandoraSingleton<UIMissionManager>
{
    public enum State
    {
        MOVING,
        SINGLE_TARGETING,
        SEQUENCE,
        COUNTER,
        SEARCH,
        ENDGAME,
        DEPLOY,
        MAX
    }

    public enum UnitInfoState
    {
        NONE,
        BUFFS,
        DEBUFFS,
        STATS,
        WEAPON_RESIST,
        MAX
    }

    public QuitGamePopup quitGamePopup;

    public ConfirmationPopupView messagePopup;

    public GameObject optionsMenu;

    private OptionsManager optionsMan;

    public UIMissionLadderController ladder;

    public UIMissionMoraleController morale;

    public UISequenceMessage leftSequenceMessage;

    public UISequenceMessage rightSequenceMessage;

    public UIInteractiveMessage interactiveMessage;

    public UITurnMessage turnMessage;

    public UIObjectivesController objectives;

    public UIUnitCombatStatsController unitCombatStats;

    public UIUnitCurrentActionController unitAction;

    public UISlideInElement unitAlternateWeapon;

    public UISlideInElement unitStats;

    public UISlideInElement unitEnchantments;

    public UISlideInElement unitEnchantmentsDebuffs;

    public UIUnitCombatStatsController targetCombatStats;

    public UISlideInElement targetAlternateWeapon;

    public UISlideInElement targetStats;

    public UISlideInElement targetEnchantments;

    public UISlideInElement targetEnchantmentsDebuffs;

    public List<UIUnitControllerChanged> currentUnitUI;

    public List<UIUnitControllerChanged> currentUnitTargetUI;

    public List<UISlideInElement> extraStats;

    public UIWheelController wheel;

    public UIInventoryController inventory;

    public UIEndGameReport endGameReport;

    public UIChatLog chatLog;

    public UIOverviewController overview;

    public UIPropsInfoController propsInfo;

    public UIDeployControls deployControls;

    public bool showExtraStats;

    private UnitController currentUnitController;

    private UnitController currentUnitTargetController;

    private Destructible currentUnitTargetDestructible;

    [HideInInspector]
    public CheapStateMachine StateMachine;

    [HideInInspector]
    public AudioSource audioSource;

    private CanvasGroupDisabler canvasDisabler;

    private bool uiVisible;

    public UnitInfoState ShowingMoreInfoUnit
    {
        get;
        private set;
    }

    public bool ShowingMoreInfoUnitAction
    {
        get;
        private set;
    }

    public bool ShowingMoreInfoMission
    {
        get;
        private set;
    }

    public bool ShowingOverview
    {
        get;
        set;
    }

    [HideInInspector]
    public UnitController CurrentUnitController
    {
        get
        {
            return currentUnitController;
        }
        set
        {
            currentUnitController = value;
            if (currentUnitController != null)
            {
                for (int i = 0; i < currentUnitUI.Count; i++)
                {
                    currentUnitUI[i].UnitChanged(currentUnitController, null);
                }
            }
        }
    }

    [HideInInspector]
    public UnitController CurrentUnitTargetController
    {
        get
        {
            return currentUnitTargetController;
        }
        set
        {
            currentUnitTargetController = value;
            if (currentUnitTargetController != null)
            {
                currentUnitTargetDestructible = null;
                for (int i = 0; i < currentUnitTargetUI.Count; i++)
                {
                    currentUnitTargetUI[i].UnitChanged(currentUnitTargetController, currentUnitController);
                }
            }
        }
    }

    [HideInInspector]
    public Destructible CurrentUnitTargetDestructible
    {
        get
        {
            return currentUnitTargetDestructible;
        }
        set
        {
            currentUnitTargetDestructible = value;
            if (currentUnitTargetDestructible != null)
            {
                currentUnitTargetController = null;
                for (int i = 0; i < currentUnitTargetUI.Count; i++)
                {
                    currentUnitTargetUI[i].UnitChanged(currentUnitTargetController, currentUnitController, currentUnitTargetDestructible);
                }
            }
        }
    }

    private void Awake()
    {
        optionsMenu = Object.Instantiate(optionsMenu);
        optionsMan = optionsMenu.GetComponentsInChildren<OptionsManager>(includeInactive: true)[0];
        optionsMan.onCloseOptionsMenu = HideOptions;
        optionsMan.onQuitGame = OnQuitGame;
        optionsMan.onSaveQuitGame = OnSaveAndQuit;
        optionsMan.SetBackButtonLoc("menu_back_to_game");
        int num = 0;
        for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count; i++)
        {
            if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands[i].PlayerTypeId == PlayerTypeId.PLAYER)
            {
                num++;
            }
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            optionsMan.SetQuitButtonLoc("menu_quit_tutorial", string.Empty);
            optionsMan.HideAltQuitOption();
            optionsMan.HideSaveAndQuitOption();
        }
        else if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign && (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish || num > 1))
        {
            optionsMan.DisableSaveAndQuitOption();
        }
        quitGamePopup.transform.SetParent(optionsMenu.transform);
        quitGamePopup.transform.localPosition = Vector3.zero;
        quitGamePopup.transform.localScale = Vector3.one;
        canvasDisabler = GetComponent<CanvasGroupDisabler>();
        audioSource = GetComponent<AudioSource>();
        StateMachine = new CheapStateMachine(7);
        StateMachine.AddState(new UIMissionMoving(this), 0);
        StateMachine.AddState(new UIMissionTarget(this), 1);
        StateMachine.AddState(new UIMissionSequence(this), 2);
        StateMachine.AddState(new UIMissionSearch(this), 4);
        StateMachine.AddState(new UIMissionEndGame(this), 5);
        StateMachine.AddState(new UIMissionDeploy(this), 6);
    }

    private void Start()
    {
        ((RectTransform)base.transform).localScale = Vector3.one;
        optionsMenu.SetActive(value: false);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_START_MOVE, delegate
        {
            ChangeState(State.MOVING);
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_INVENTORY, delegate
        {
            ChangeState(State.SEARCH);
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_START_SINGLE_TARGETING, delegate
        {
            ChangeState(State.SINGLE_TARGETING);
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_END, delegate
        {
            ChangeState(State.ENDGAME);
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_DEPLOY, delegate
        {
            ChangeState(State.DEPLOY);
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_CHANGED, OnCurrentUnitChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_TARGET_CHANGED, OnCurrentUnitTargetChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_TARGET_DESTUCTIBLE_CHANGED, OnCurrentTargetDestructibleChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SEQUENCE_STARTED, OnSequenceStarted);
        ladder.OnDisable();
        wheel.OnDisable();
        turnMessage.OnDisable();
        morale.OnDisable();
        leftSequenceMessage.OnDisable();
        rightSequenceMessage.OnDisable();
        objectives.OnDisable();
        endGameReport.gameObject.SetActive(value: false);
        unitCombatStats.OnDisable();
        unitAction.OnDisable();
        unitAlternateWeapon.OnDisable();
        unitEnchantments.OnDisable();
        unitEnchantmentsDebuffs.OnDisable();
        unitStats.OnDisable();
        targetCombatStats.OnDisable();
        targetAlternateWeapon.OnDisable();
        targetEnchantments.OnDisable();
        targetEnchantmentsDebuffs.OnDisable();
        targetStats.OnDisable();
        inventory.OnDisable();
        chatLog.Setup();
        chatLog.OnDisable();
        overview.OnDisable();
        propsInfo.OnDisable();
        deployControls.OnDisable();
        ShowingMoreInfoMission = true;
        OnSetOptionFullUI(resetUI: false);
    }

    public void OnSetOptionFullUI(bool resetUI)
    {
        if (PandoraSingleton<GameManager>.Instance.Options.displayFullUI)
        {
            ShowingMoreInfoUnit = UnitInfoState.BUFFS;
            ShowingMoreInfoUnitAction = true;
            chatLog.ShowLog();
        }
        else
        {
            ShowingMoreInfoUnit = UnitInfoState.NONE;
            ShowingMoreInfoUnitAction = false;
            chatLog.HideLog();
        }
        if (resetUI)
        {
            PandoraSingleton<NoticeManager>.instance.SendNotice(Notices.GAME_MORE_INFO_UNIT_ACTION_TOGGLE, ShowingMoreInfoUnitAction);
            ShowUnitExtraStats();
            ShowTargetExtraStats();
            ShowObjectives();
        }
    }

    public void OnDestroy()
    {
        StateMachine.Destroy();
    }

    private void ChangeState(State state)
    {
        int activeStateId = StateMachine.GetActiveStateId();
        if (activeStateId == 5)
        {
            PandoraDebug.LogWarning("Trying to enter state " + state.ToString() + " while in state endgame");
        }
        else
        {
            StateMachine.ChangeState((int)state);
        }
    }

    private void OnSequenceStarted()
    {
        ChangeState(State.SEQUENCE);
    }

    private void OnCurrentUnitChanged()
    {
        CurrentUnitController = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        if (CurrentUnitController == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.LADDER_UNIT_CHANGED, CurrentUnitController);
        }
    }

    private void OnCurrentUnitTargetChanged()
    {
        CurrentUnitTargetController = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
    }

    private void OnCurrentTargetDestructibleChanged()
    {
        CurrentUnitTargetDestructible = (Destructible)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
    }

    public bool IsNoticeCurrentUnitController()
    {
        UnitController y = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        return CurrentUnitController == y;
    }

    private void Update()
    {
        StateMachine.Update();
        if (StateMachine.GetActiveStateId() != 2 && StateMachine.GetActiveStateId() != 6)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("menu"))
            {
                if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK || CurrentUnitController == null || !CurrentUnitController.IsPlayed() || (!CurrentUnitController.IsCurrentState(UnitController.State.AOE_TARGETING) && !CurrentUnitController.IsCurrentState(UnitController.State.SINGLE_TARGETING) && !CurrentUnitController.IsCurrentState(UnitController.State.LINE_TARGETING) && !CurrentUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE) && !CurrentUnitController.IsCurrentState(UnitController.State.INTERACTIVE_TARGET) && StateMachine.GetActiveStateId() == 0 && (CurrentUnitController.CurrentAction == null || !CurrentUnitController.CurrentAction.waitForConfirmation)))
                {
                    ShowOptions();
                }
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("wheel") && CurrentUnitController != null && !CurrentUnitController.CurrentAction.waitForConfirmation && (CurrentUnitController.IsCurrentState(UnitController.State.MOVE) || CurrentUnitController.IsCurrentState(UnitController.State.ENGAGED)))
            {
                wheel.Show();
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("hide_gui", -1))
        {
            ToggleUI();
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("show_chat") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("show_chat", 3))
        {
            ShowChat();
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("show_action_log") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("show_action_log", 7))
        {
            ShowActionLog();
        }
        if (Input.GetKeyUp(KeyCode.F2))
        {
            CurrentUnitController.SetCombatCircle2(CurrentUnitController);
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("more_info_unit") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("more_info_unit", 6))
        {
            if (++ShowingMoreInfoUnit >= UnitInfoState.MAX)
            {
                ShowingMoreInfoUnit = UnitInfoState.NONE;
            }
            if (StateMachine.GetActiveStateId() != 2)
            {
                ShowUnitExtraStats();
                ShowTargetExtraStats();
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_buffs") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_buffs", 6))
        {
            if (ShowingMoreInfoUnit == UnitInfoState.BUFFS)
            {
                ShowingMoreInfoUnit = UnitInfoState.DEBUFFS;
            }
            else if (ShowingMoreInfoUnit == UnitInfoState.DEBUFFS)
            {
                ShowingMoreInfoUnit = UnitInfoState.NONE;
            }
            else
            {
                ShowingMoreInfoUnit = UnitInfoState.BUFFS;
            }
            if (StateMachine.GetActiveStateId() != 2)
            {
                ShowUnitExtraStats();
                ShowTargetExtraStats();
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_stats") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_stats", 6))
        {
            if (ShowingMoreInfoUnit == UnitInfoState.STATS)
            {
                ShowingMoreInfoUnit = UnitInfoState.NONE;
            }
            else
            {
                ShowingMoreInfoUnit = UnitInfoState.STATS;
            }
            if (StateMachine.GetActiveStateId() != 2)
            {
                ShowUnitExtraStats();
                ShowTargetExtraStats();
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_resists") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("unit_info_resists", 6))
        {
            if (ShowingMoreInfoUnit == UnitInfoState.WEAPON_RESIST)
            {
                ShowingMoreInfoUnit = UnitInfoState.NONE;
            }
            else
            {
                ShowingMoreInfoUnit = UnitInfoState.WEAPON_RESIST;
            }
            if (StateMachine.GetActiveStateId() != 2)
            {
                ShowUnitExtraStats();
                ShowTargetExtraStats();
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("more_info_unit_action"))
        {
            ShowingMoreInfoUnitAction = !ShowingMoreInfoUnitAction;
            PandoraSingleton<NoticeManager>.instance.SendNotice(Notices.GAME_MORE_INFO_UNIT_ACTION_TOGGLE, ShowingMoreInfoUnitAction);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("more_info_mission"))
        {
            ShowingMoreInfoMission = !ShowingMoreInfoMission;
            ShowObjectives();
        }
    }

    private void ToggleUI()
    {
        if (optionsMenu.activeSelf)
        {
            uiVisible = !uiVisible;
            return;
        }
        canvasDisabler.enabled = !canvasDisabler.enabled;
        PandoraDebug.LogInfo("Gui is now " + canvasDisabler.enabled, "Commands", this);
    }

    private void ShowChat()
    {
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            chatLog.ShowChat(blockInput: true);
        }
    }

    private void ShowActionLog()
    {
        chatLog.ToggleLogDisplay();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    public void ShowObjectives()
    {
        ShowingMoreInfoMission |= (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign && !PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto);
        if (ShowingMoreInfoMission)
        {
            objectives.OnEnable();
        }
        else
        {
            objectives.OnDisable();
        }
    }

    public void ShowUnitExtraStats()
    {
        if (CurrentUnitController != null && CurrentUnitController.IsPlayed())
        {
            switch (ShowingMoreInfoUnit)
            {
                case UnitInfoState.NONE:
                    unitAlternateWeapon.Hide();
                    unitStats.Hide();
                    unitEnchantments.Hide();
                    unitEnchantmentsDebuffs.Hide();
                    break;
                case UnitInfoState.BUFFS:
                    unitAlternateWeapon.Hide();
                    unitStats.Hide();
                    unitEnchantments.Show();
                    unitEnchantmentsDebuffs.Hide();
                    break;
                case UnitInfoState.DEBUFFS:
                    unitAlternateWeapon.Hide();
                    unitStats.Hide();
                    unitEnchantments.Hide();
                    unitEnchantmentsDebuffs.Show();
                    break;
                case UnitInfoState.STATS:
                    unitAlternateWeapon.Hide();
                    unitEnchantments.Hide();
                    unitEnchantmentsDebuffs.Hide();
                    unitStats.Show();
                    break;
                case UnitInfoState.WEAPON_RESIST:
                    unitStats.Hide();
                    unitEnchantments.Hide();
                    unitEnchantmentsDebuffs.Hide();
                    unitAlternateWeapon.Show();
                    break;
                default:
                    PandoraDebug.LogWarning("Unsupported UnitInfoState, this should not happen, showing no unit infos.", "UI", this);
                    break;
            }
        }
        else
        {
            unitStats.Hide();
            unitEnchantments.Hide();
            unitEnchantmentsDebuffs.Hide();
            unitAlternateWeapon.Hide();
        }
    }

    public void ShowTargetExtraStats()
    {
        if ((ShowingOverview && CurrentUnitTargetController != null) || (CurrentUnitController != null && CurrentUnitTargetController != null && CurrentUnitController.IsPlayed() && StateMachine.GetActiveStateId() == 1))
        {
            switch (ShowingMoreInfoUnit)
            {
                case UnitInfoState.NONE:
                    targetAlternateWeapon.Hide();
                    targetStats.Hide();
                    targetEnchantments.Hide();
                    targetEnchantmentsDebuffs.Hide();
                    break;
                case UnitInfoState.BUFFS:
                    targetAlternateWeapon.Hide();
                    targetStats.Hide();
                    targetEnchantments.Show();
                    targetEnchantmentsDebuffs.Hide();
                    break;
                case UnitInfoState.DEBUFFS:
                    targetAlternateWeapon.Hide();
                    targetStats.Hide();
                    targetEnchantments.Hide();
                    targetEnchantmentsDebuffs.Show();
                    break;
                case UnitInfoState.STATS:
                    targetAlternateWeapon.Hide();
                    targetStats.Show();
                    targetEnchantments.Hide();
                    targetEnchantmentsDebuffs.Hide();
                    break;
                case UnitInfoState.WEAPON_RESIST:
                    targetAlternateWeapon.Show();
                    targetStats.Hide();
                    targetEnchantments.Hide();
                    targetEnchantmentsDebuffs.Hide();
                    break;
                default:
                    PandoraDebug.LogWarning("Unsupported UnitInfoState, this should not happen, showing no unit infos.", "UI", this);
                    break;
            }
        }
        else
        {
            targetAlternateWeapon.Hide();
            targetStats.Hide();
            targetEnchantments.Hide();
        }
    }

    public void HideUnitStats()
    {
        CurrentUnitController = null;
        CurrentUnitTargetController = null;
        unitCombatStats.OnDisable();
        targetCombatStats.OnDisable();
        ShowUnitExtraStats();
        ShowTargetExtraStats();
    }

    public void SetPropsInfo(Sprite icon, string title)
    {
        propsInfo.OnEnable();
        propsInfo.Set(icon, title);
    }

    public void HidePropsInfo()
    {
        propsInfo.OnDisable();
    }

    public void ShowOptions()
    {
        if (optionsMenu.activeSelf)
        {
            return;
        }
        uiVisible = canvasDisabler.enabled;
        canvasDisabler.enabled = false;
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.MENU);
        optionsMenu.SetActive(value: true);
        if (((RectTransform)optionsMenu.transform).localScale == Vector3.zero)
        {
            ((RectTransform)optionsMenu.transform).localScale = Vector3.one;
        }
        if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            if (PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().MoralRatio > PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
            {
                optionsMan.DisableAltQuitOption();
            }
            else
            {
                optionsMan.DisableQuitOption();
            }
        }
        optionsMan.OnShow();
        optionsMan.butExit.RefreshImage();
    }

    public void HideOptions()
    {
        if (optionsMenu.activeSelf)
        {
            canvasDisabler.enabled = uiVisible;
            PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: false);
            PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.MENU);
            optionsMenu.SetActive(value: false);
            optionsMan.OnHide();
            quitGamePopup.Hide();
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.OPTIONS_CLOSED);
        }
    }

    public void OnSaveAndQuit()
    {
        quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_save_and_quit_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_save_and_quit_desc"), SaveAndQuit);
    }

    private void SaveAndQuit(bool confirm)
    {
        if (confirm)
        {
            PandoraSingleton<MissionManager>.Instance.SaveAndQuit();
        }
        else
        {
            optionsMan.butSaveAndQuit.SetSelected(force: true);
        }
    }

    public void OnQuitGame(bool altQuit)
    {
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_desc_no_penalty"), QuitGame);
        }
        else if (altQuit)
        {
            if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish)
            {
                quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_exhibition_desc"), QuitGame);
            }
            else
            {
                quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_desc"), QuitGame);
            }
        }
        else if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish)
        {
            quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_voluntary_rout_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_desc_no_penalty"), QuitGame);
        }
        else
        {
            quitGamePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_voluntary_rout_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_quit_mission_voluntary_rout_desc"), QuitGame);
        }
    }

    private void QuitGame(bool confirm)
    {
        if (confirm)
        {
            HideOptions();
            PandoraSingleton<MissionManager>.Instance.NetworkMngr.SendForfeitMission(PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().idx);
        }
        else
        {
            optionsMan.butQuit.SetSelected(force: true);
        }
    }
}
