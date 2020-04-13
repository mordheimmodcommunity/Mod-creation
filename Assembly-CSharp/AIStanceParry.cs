using RAIN.Action;
using RAIN.Core;

public class AIStanceParry : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "StanceParry";
        success = unitCtrlr.GetAction(SkillId.BASE_STANCE_PARRY).Available;
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_STANCE_PARRY);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
