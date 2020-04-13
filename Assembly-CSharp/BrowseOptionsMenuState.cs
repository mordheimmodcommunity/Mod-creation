using System.Collections.Generic;

public class BrowseOptionsMenuState : ICheapState
{
    private OptionsMenuState controller;

    private List<OptionsMenuState.State> options;

    private int selection;

    public BrowseOptionsMenuState(OptionsMenuState ctrlr)
    {
        controller = ctrlr;
        options = new List<OptionsMenuState.State>();
        options.Add(OptionsMenuState.State.GRAPHICS);
        options.Add(OptionsMenuState.State.AUDIO);
        options.Add(OptionsMenuState.State.GAMEPLAY);
        options.Add(OptionsMenuState.State.CONTROL);
        if (controller.canQuit)
        {
            options.Add(OptionsMenuState.State.TO_MAIN_MENU);
            options.Add(OptionsMenuState.State.QUIT);
        }
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_ACTION, InputAction);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_CANCEL, InputCancel);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_OPTIONS_CAT, InputCat);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_OPTIONS, !controller.canQuit, v2: true);
        UpdateSelection(0);
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_ACTION, InputAction);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_CANCEL, InputCancel);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_OPTIONS_CAT, InputCat);
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
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("v"))
        {
            UpdateSelection(-1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("v"))
        {
            UpdateSelection(1);
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void InputAction()
    {
        controller.GoTo((OptionsMenuState.State)selection);
    }

    private void InputCancel()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_OPTIONS, !controller.canQuit, v2: false);
        controller.ExitState();
    }

    private void InputCat()
    {
        int num = (int)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        if (num != selection)
        {
            UpdateSelection(num - selection);
        }
    }

    private void UpdateSelection(int direction)
    {
        selection += direction;
        if (selection >= options.Count)
        {
            selection = 0;
        }
        else if (selection < 0)
        {
            selection = options.Count - 1;
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_OPTIONS_SELECTED, selection);
    }
}
