using RAIN.Core;

public class AIBeenShot : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "BeenShot";
        success = unitCtrlr.beenShot;
        unitCtrlr.beenShot = false;
    }
}
