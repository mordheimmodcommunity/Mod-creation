using RAIN.Action;
using RAIN.Core;

public class AISwitchWeaponSet : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "SwitchWeaponSet";
        success = unitCtrlr.GetAction(SkillId.BASE_SWITCH_WEAPONS).Available;
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_SWITCH_WEAPONS);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
