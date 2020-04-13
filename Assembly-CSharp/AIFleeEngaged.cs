using RAIN.Action;
using RAIN.Core;

public class AIFleeEngaged : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "FleeEngaged";
        unitCtrlr.AICtrlr.AddEngagedToExcluded();
        success = (unitCtrlr.AICtrlr.disengageCount == 0 && unitCtrlr.GetAction(SkillId.BASE_FLEE).Available);
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_FLEE);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
