using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIConsSkillSpell : AIBaseAction
{
    protected List<UnitController> allies;

    protected List<UnitController> enemies;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIConsSkillSpell";
    }

    protected override bool IsValid(ActionStatus action, UnitController target)
    {
        if (allies == null)
        {
            unitCtrlr.GetAlliesEnemies(out allies, out enemies);
        }
        bool flag = allies.IndexOf(target) != -1 || target == unitCtrlr;
        bool flag2 = enemies.IndexOf(target) != -1;
        if (unitCtrlr.unit.Id == UnitId.MANTICORE)
        {
            float num = Vector3.Dot(unitCtrlr.transform.forward, target.transform.position - unitCtrlr.transform.position);
            if (num < 0.25f)
            {
                return false;
            }
        }
        return (action.skillData.EffectTypeId == EffectTypeId.BUFF && flag) || (action.skillData.EffectTypeId == EffectTypeId.DEBUFF && flag2) || (action.skillData.EffectTypeId == EffectTypeId.INSTANT && action.skillData.WoundMax == 0 && flag) || (action.skillData.EffectTypeId == EffectTypeId.INSTANT && action.skillData.WoundMax != 0 && flag2);
    }

    protected override bool ByPassLimit(UnitController target)
    {
        return true;
    }

    protected override bool IsBetter(int currentVal, int val)
    {
        return false;
    }

    protected override int GetCriteriaValue(UnitController target)
    {
        return 0;
    }

    protected override List<UnitActionId> GetRelatedActions()
    {
        return AIController.consSkillSpellActions;
    }
}
