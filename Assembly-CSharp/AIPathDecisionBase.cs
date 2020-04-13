using RAIN.Action;
using RAIN.Core;

public abstract class AIPathDecisionBase : AIBase
{
    private ActionResult currentResult;

    public override void Start(AI ai)
    {
        //IL_0014: Unknown result type (might be due to invalid IL or missing references)
        base.Start(ai);
        base.actionName = "AIPathClosestOverwatch";
        currentResult = (ActionResult)1;
        float dist = unitCtrlr.unit.Movement * 2;
        unitCtrlr.AICtrlr.FindPath(GetDecisionId(), dist, AllPathChecked);
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        return currentResult;
    }

    private void AllPathChecked(bool pathSuccess)
    {
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        pathSuccess &= (unitCtrlr.AICtrlr.currentPath != null);
        success = pathSuccess;
        currentResult = (ActionResult)((!success) ? 2 : 0);
    }

    protected abstract DecisionPointId GetDecisionId();
}
