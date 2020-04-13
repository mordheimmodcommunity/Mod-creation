public class ToMainMenuOptionsMenuState : ICheapState
{
    private OptionsMenuState controller;

    public ToMainMenuOptionsMenuState(OptionsMenuState ctrlr)
    {
        controller = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_ACTION, InputAction);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_CANCEL, InputCancel);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT);
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_ACTION, InputAction);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_CANCEL, InputCancel);
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            InputAction();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel"))
        {
            InputCancel();
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void InputAction()
    {
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT_CONF, v1: true);
    }

    private void InputCancel()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT_CONF, v1: false);
        controller.GoTo(OptionsMenuState.State.BROWSE);
    }
}
