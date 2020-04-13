using RAIN.Action;
using RAIN.Core;

public class AIDisengageEngaged : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "DisengageEngaged";
        unitCtrlr.AICtrlr.AddEngagedToExcluded();
        success = (unitCtrlr.AICtrlr.disengageCount == 0 && unitCtrlr.GetAction(SkillId.BASE_DISENGAGE).Available);
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.AICtrlr.disengageCount++;
            unitCtrlr.SendSkill(SkillId.BASE_DISENGAGE);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
