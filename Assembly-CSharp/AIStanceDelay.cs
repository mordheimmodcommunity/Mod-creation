using RAIN.Action;
using RAIN.Core;

public class AIStanceDelay : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "StanceDelay";
        success = unitCtrlr.GetAction(SkillId.BASE_DELAY).Available;
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_DELAY);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
