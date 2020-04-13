using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public class AIKillUnit : AIBase
{
    private SkillId skillId;

    private UnitController target;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "KillUnit";
        target = null;
        skillId = ((!unitCtrlr.Engaged) ? SkillId.BASE_SHOOT : SkillId.BASE_ATTACK);
        ActionStatus action = unitCtrlr.GetAction(skillId);
        action.UpdateAvailable();
        int num = (action.GetMinDamage() + action.GetMaxDamage()) / 2;
        List<UnitController> targets = action.Targets;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].unit.CurrentWound <= num * 2)
            {
                target = targets[i];


                break;
            }
        }
        success = (target != null);
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkillSingleTarget(skillId, target);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
