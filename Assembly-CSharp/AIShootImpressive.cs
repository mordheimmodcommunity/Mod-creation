using RAIN.Core;

public class AIShootImpressive : AIShootBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "ShootImpressive";
	}

	protected override bool ByPassLimit(UnitController current)
	{
		return current.unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE;
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		return false;
	}

	protected override int GetCriteriaValue(UnitController current)
	{
		return 0;
	}
}
