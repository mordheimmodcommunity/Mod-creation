using System.Collections.Generic;

public class AIShootBase : AIBaseAction
{
	protected override bool IsValid(ActionStatus action, UnitController target)
	{
		return target.GetWarband().teamIdx != unitCtrlr.GetWarband().teamIdx;
	}

	protected override bool ByPassLimit(UnitController current)
	{
		return true;
	}

	protected override int GetCriteriaValue(UnitController current)
	{
		return 0;
	}

	protected override bool IsBetter(int currentDist, int dist)
	{
		return true;
	}

	protected override List<UnitActionId> GetRelatedActions()
	{
		return AIController.shootActions;
	}
}
