using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIConsSkillReserve : AIConsSkillSpell
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AIConsSkillReserve";
	}

	protected override bool IsValid(ActionStatus action, UnitController target)
	{
		return action.OffensePoints <= Mathf.Max(unitCtrlr.unit.CurrentOffensePoints - 2, 0) && action.StrategyPoints <= Mathf.Max(unitCtrlr.unit.CurrentStrategyPoints - 2, 0) && base.IsValid(action, target);
	}

	protected override List<UnitActionId> GetRelatedActions()
	{
		return AIController.consSkillActions;
	}

	public override void Stop(AI ai)
	{
		base.Stop(ai);
	}
}
