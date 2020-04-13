using RAIN.Core;

public class AIIsInTargetSearchPoint : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "IsInTargetSearchPoint";
        success = false;
        foreach (SearchPoint interactivePoint in unitCtrlr.interactivePoints)
        {
            if (interactivePoint == unitCtrlr.AICtrlr.targetSearchPoint)
            {
                success = true;
                break;
            }
        }
    }
}
