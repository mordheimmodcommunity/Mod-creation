using RAIN.Core;

public class AIHasBetterDodge : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "HasBetterDodge";
        success = (unitCtrlr.unit.DodgeRoll >= unitCtrlr.unit.ParryingRoll);
    }
}
