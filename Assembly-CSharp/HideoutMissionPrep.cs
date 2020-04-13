public class HideoutMissionPrep : WarbandManagementBaseState
{
    public HideoutMissionPrep(HideoutManager mng, HideoutCamAnchor anchor)
        : base(mng, ModuleId.MISSION_PREP)
    {
    }

    public override void Enter(int iFrom)
    {
        base.Enter(iFrom);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.MENU);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModule(true, ModuleId.TITLE);
        TitleModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<TitleModule>(ModuleId.TITLE);
        moduleLeft.Set("units_selection_title");
    }

    public override void Exit(int iTo)
    {
        PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.MENU);
    }

    protected override void SetupDefaultButtons()
    {
        base.SetupDefaultButtons();
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_back", 6, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnToMission, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "Start Mission", 6);
        PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(StartMission, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
    }

    private void ReturnToMission()
    {
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(12);
    }

    private void StartMission()
    {
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION_CAMPAIGN);
    }
}
