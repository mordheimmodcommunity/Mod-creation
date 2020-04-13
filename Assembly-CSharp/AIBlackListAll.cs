using RAIN.Action;
using RAIN.Core;

public class AIBlackListAll : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "BlackListAll";
        unitCtrlr.GetWarband().BlackListAll();
    }

    public override ActionResult Execute(AI ai)
    {
        return (ActionResult)0;
    }
}
