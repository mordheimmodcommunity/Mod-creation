using RAIN.Core;

public class AIShootHealthiest : AIShootBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "ShootHealthiest";
	}

	protected override bool ByPassLimit(UnitController current)
	{
		return false;
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		return currentVal < val;
	}

	protected override int GetCriteriaValue(UnitController current)
	{
		return current.unit.CurrentWound;
	}
}
