using RAIN.Core;
using RAIN.Memory;
using RAIN.Representation;

public class AIIsTargetHealthBelow : AIBase
{
    public Expression expr;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AICanTargetCounter";
        float num = expr.Evaluate<float>(0f, (RAINMemory)null);
        success = false;
        if (unitCtrlr.defenderCtrlr != null)
        {
            float num2 = unitCtrlr.defenderCtrlr.unit.CurrentWound / unitCtrlr.defenderCtrlr.unit.Wound;
            success = (num2 < num);
        }
    }
}
