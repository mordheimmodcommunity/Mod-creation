using RAIN.Core;

public class AIHasMeleeWpn : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "HasMeleeWpn";
        success = unitCtrlr.HasClose();
    }
}
