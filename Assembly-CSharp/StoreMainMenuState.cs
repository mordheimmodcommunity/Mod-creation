public class StoreMainMenuState : UIStateMonoBehaviour<MainMenuController>
{
    public override int StateId => 2;

    public override void StateEnter()
    {
    }

    public override void OnInputCancel()
    {
        base.StateMachine.GoToPrev();
    }

    public override void StateExit()
    {
    }
}
