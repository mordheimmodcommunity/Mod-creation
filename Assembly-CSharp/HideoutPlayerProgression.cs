using System.Collections;

public class HideoutPlayerProgression : ICheapState
{
    private HideoutCamAnchor camAnchor;

    private PlayerTasksSkillsModule taskList;

    private PlayerTaskDescModule taskDesc;

    private WarbandSkillsWheelModule skillWheel;

    private WarbandSkillDescModule skillDesc;

    private Warband warband;

    private WarbandSkill currentSkill;

    private int currentWheelIndex;

    private WarbandTabsModule warbandTabs;

    private bool once = true;

    public HideoutPlayerProgression(HideoutManager mng, HideoutCamAnchor anchor)
    {
        camAnchor = anchor;
    }

    public void Destroy()
    {
    }

    public void Enter(int iFrom)
    {
        warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        currentWheelIndex = 0;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        UnitMenuController dramatis = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetDramatis();
        PandoraSingleton<HideoutManager>.Instance.progressNode.SetContent(dramatis);
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(PandoraSingleton<HideoutManager>.Instance.progressNode.transform, 1.25f);
        dramatis.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WHEEL_WARBAND_SKILLS, ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.PLAYER_SHEET, ModuleId.PLAYER_RANK_DESC);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.TASK_LIST);
        warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
        warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
        warbandTabs.SetCurrentTab(HideoutManager.State.PLAYER_PROGRESSION);
        warbandTabs.Refresh();
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<PlayerSheetModule>(ModuleId.PLAYER_SHEET).Refresh();
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<PlayerRankDescModule>(ModuleId.PLAYER_RANK_DESC).Refresh();
        taskDesc = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<PlayerTaskDescModule>(ModuleId.TASK_DESC);
        skillWheel = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSkillsWheelModule>(ModuleId.WHEEL_WARBAND_SKILLS);
        skillDesc = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSkillDescModule>(ModuleId.WARBAND_SKILL_DESC);
        taskList = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<PlayerTasksSkillsModule>(ModuleId.TASK_LIST);
        taskList.Set(OnTabChanged, OnTaskSelected, OnSkillSelected, OnSkillConfirmed);
        taskList.tabs.tabs[0].SetOn();
        taskList.SetTab(PlayerProgressTab.TASKS, callBack: true);
        once = true;
    }

    private void SetButtonsForPlayerTask()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_back", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        }, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        SetLastButtons();
    }

    private void SetButtonsForWheelSelection()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_back", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        }, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        SetLastButtons();
    }

    private void SetButtonsForSkillSelection()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_return_select_slot", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnSelectSlot, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        SetLastButtons();
    }

    private void SetLastButtons()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button3.SetAction("show_chat", "menu_respec");
        PandoraSingleton<HideoutTabManager>.Instance.button3.OnAction(DisplayRespecPopup, mouseOnly: false);
        RefreshRespecButton();
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
    }

    public void ReturnSelectSlot()
    {
        taskList.list.ClearList();
        taskList.RefreshAvailablePoints();
        skillWheel.slots[currentWheelIndex].SetSelected(force: true);
    }

    private void RefreshRespecButton()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button3.label.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec", warband.GetWarbandSave().availaibleRespec.ToString()));
    }

    public void Exit(int iTo)
    {
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        if (once)
        {
            once = false;
            PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.PROGRESSION);
        }
    }

    private void DisplayRespecPopup()
    {
        int availaibleRespec = warband.GetWarbandSave().availaibleRespec;
        int @int = Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR);
        int num = warband.GetWarbandSave().currentDate / @int;
        int num2 = @int - (warband.GetWarbandSave().currentDate - num * @int);
        if (warband.GetPlayerSkills().Count == 0)
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_no_skills_desc", num2.ToString()), null, hideButtons: false, hideCancel: true);
        }
        else if (availaibleRespec == 0)
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_none_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_none_desc", num2.ToString()), null, hideButtons: false, hideCancel: true);
        }
        else
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_respec_desc", num2.ToString()), Respec);
        }
    }

    private void Respec(bool confirm)
    {
        if (confirm)
        {
            warband.ResetPlayerSkills();
            PandoraSingleton<HideoutManager>.Instance.StartCoroutine(NextFrameReturn());
            RefreshRespecButton();
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        }
    }

    private IEnumerator NextFrameReturn()
    {
        yield return null;
        if (skillWheel.gameObject.activeInHierarchy)
        {
            skillWheel.SetSelected(force: true);
        }
        yield return null;
        OnTabChanged(skillWheel.gameObject.activeInHierarchy ? PlayerProgressTab.SKILLS : PlayerProgressTab.TASKS);
        if (skillWheel.gameObject.activeInHierarchy)
        {
            currentWheelIndex = 0;
            ReturnSelectSlot();
        }
    }

    private void OnTabChanged(PlayerProgressTab tab)
    {
        switch (tab)
        {
            case PlayerProgressTab.SKILLS:
                taskDesc.gameObject.SetActive(value: false);
                skillWheel.Set(OnWheelSkillSelected, OnWheelSkillConfirmed);
                skillWheel.slots[0].SetSelected();
                break;
            case PlayerProgressTab.TASKS:
                skillWheel.gameObject.SetActive(value: false);
                skillDesc.gameObject.SetActive(value: false);
                taskList.SetFocus(focus: true);
                SetButtonsForPlayerTask();
                break;
        }
    }

    private void OnTaskSelected(AchievementCategory task)
    {
        taskDesc.Set(task);
    }

    private void OnWheelSkillSelected(int idx, WarbandSkill skill)
    {
        taskList.SetCurrentSkill(skill);
        taskList.SetFocus(focus: false);
        skillDesc.Set(idx, skill);
        currentWheelIndex = idx;
        SetButtonsForWheelSelection();
    }

    private void OnWheelSkillConfirmed()
    {
        taskList.SetSkillsList();
        if (taskList.list.items.Count > 0)
        {
            taskList.SetFocus(focus: true);
        }
    }

    private void OnSkillSelected(WarbandSkill skill)
    {
        currentSkill = skill;
        skillDesc.Set(0, skill);
        SetButtonsForSkillSelection();
    }

    private void OnSkillConfirmed(WarbandSkill skill)
    {
        if (skill.Data.Points <= warband.GetPlayerSkillsAvailablePoints())
        {
            currentSkill = skill;
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warband_skill_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warband_skill_confirm_desc", skill.LocalizedName), OnSkillPopupConfirmed);
        }
    }

    private void OnSkillPopupConfirmed(bool isConfirm)
    {
        if (isConfirm)
        {
            warband.LearnSkill(currentSkill);
            taskList.SetTab(PlayerProgressTab.SKILLS, callBack: true);
            ReturnSelectSlot();
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        }
    }
}
