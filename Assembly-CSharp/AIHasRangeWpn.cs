using RAIN.Core;

public class AIHasRangeWpn : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "HasRangeWpn";
        success = unitCtrlr.HasRange();
    }
}
