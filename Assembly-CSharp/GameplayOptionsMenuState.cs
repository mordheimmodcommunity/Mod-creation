public class GameplayOptionsMenuState : ICheapState
{
    private OptionsMenuState controller;

    private int selectionX;

    private int selectionY;

    public GameplayOptionsMenuState(OptionsMenuState ctrlr)
    {
        controller = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_CANCEL, InputCancel);
        selectionX = 0;
        selectionY = 0;
        UpdateSelection(0, 0);
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_CANCEL, InputCancel);
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("v"))
        {
            UpdateSelection(0, -1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("v"))
        {
            UpdateSelection(0, 1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("h"))
        {
            UpdateSelection(-1, 0);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("h"))
        {
            UpdateSelection(1, 0);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
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
    }

    private void InputCancel()
    {
        controller.GoTo(OptionsMenuState.State.BROWSE);
    }

    private void UpdateSelection(int x, int y)
    {
    }
}
