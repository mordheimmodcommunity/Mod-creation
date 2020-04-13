using RAIN.Action;
using RAIN.Core;

public class AIReloadWeapon : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "ReloadWeapon";
        success = unitCtrlr.GetAction(SkillId.BASE_RELOAD).Available;
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_RELOAD);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
