using UnityEngine;

public class OptionsMainMenuState : UIStateMonoBehaviour<MainMenuController>
{
    public ButtonGroup butExit;

    private OptionsManager optionsMan;

    public GameObject camPos;

    public override int StateId => 3;

    public override void Awake()
    {
        base.Awake();
        optionsMan = canvasGroup.GetComponentInChildren<OptionsManager>();
        optionsMan.onCloseOptionsMenu = OnCloseOptions;
        optionsMan.SetBackButtonLoc("menu_back_main_menu");
        optionsMan.HideQuitOption();
    }

    public override void StateEnter()
    {
        Show(visible: true);
        base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
        base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
        base.StateMachine.camManager.Transition();
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.MENU);
        optionsMan.OnShow();
    }

    public void OnCloseOptions()
    {
        optionsMan.OnHide();
        base.StateMachine.ChangeState(MainMenuController.State.MAIN_MENU);
    }

    public override void StateExit()
    {
        Show(visible: false);
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.MENU);
    }
}
