using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public class AIAttackIdol : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AttackIdol";
    }

    public override ActionResult Execute(AI ai)
    {
        ActionStatus action = unitCtrlr.GetAction(SkillId.BASE_ATTACK);
        int teamIdx = unitCtrlr.GetWarband().teamIdx;
        if (action.Destructibles.Count > 0)
        {
            List<Destructible> list = new List<Destructible>(action.Destructibles);
            for (int num = list.Count - 1; num >= 0; num--)
            {
                if (list[num] == null || list[num].Owner == null || list[num].Owner.GetWarband().teamIdx == teamIdx)
                {
                    list.RemoveAt(num);
                }
            }
            if (list.Count > 0)
            {
                int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, action.Destructibles.Count);
                unitCtrlr.SendSkillSingleDestructible(SkillId.BASE_ATTACK, action.Destructibles[index]);
                success = true;
                return (ActionResult)0;
            }
        }
        success = false;
        return (ActionResult)2;
    }
}
