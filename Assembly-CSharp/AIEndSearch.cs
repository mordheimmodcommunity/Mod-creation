using RAIN.Action;
using RAIN.Core;

public class AIEndSearch : AIBase
{
    private bool inSequence;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "EndSearch";
        unitCtrlr.AICtrlr.targetSearchPoint = null;
        unitCtrlr.AICtrlr.atDestination = false;
        unitCtrlr.StateMachine.ChangeState(9);
        unitCtrlr.AICtrlr.GotoPreviousMode();
        unitCtrlr.SendInventoryDone();
    }

    public override ActionResult Execute(AI ai)
    {
        return (ActionResult)0;
    }
}
