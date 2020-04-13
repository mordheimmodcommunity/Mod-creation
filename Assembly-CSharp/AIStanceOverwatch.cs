using RAIN.Action;
using RAIN.Core;

public class AIStanceOverwatch : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "StanceOverwatch";
        success = unitCtrlr.GetAction(SkillId.BASE_STANCE_OVERWATCH).Available;
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_STANCE_OVERWATCH);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
