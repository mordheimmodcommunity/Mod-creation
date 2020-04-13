using RAIN.Core;

public class AIIsInventoryFull : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "IsInventoryFull";
        success = unitCtrlr.unit.IsInventoryFull();
    }
}
