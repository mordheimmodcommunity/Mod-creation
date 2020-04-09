using RAIN.Core;

public class AIAttackHealthiest : AIAttackBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AttackHealthiest";
	}

	protected override bool ByPassLimit(UnitController target)
	{
		return false;
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		return currentVal > val;
	}

	protected override int GetCriteriaValue(UnitController target)
	{
		return target.unit.CurrentWound;
	}
}
