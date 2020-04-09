using System;
using System.Collections.Generic;

public class AIAttackBase : AIBaseAction
{
	protected override bool IsValid(ActionStatus action, UnitController target)
	{
		return true;
	}

	protected override bool ByPassLimit(UnitController target)
	{
		throw new NotImplementedException();
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		throw new NotImplementedException();
	}

	protected override int GetCriteriaValue(UnitController target)
	{
		throw new NotImplementedException();
	}

	protected override List<UnitActionId> GetRelatedActions()
	{
		return AIController.attackActions;
	}
}
