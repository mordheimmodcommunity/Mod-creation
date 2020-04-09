using RAIN.Core;

public class AIShootEnemyDown : AIShootBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "ShootEnemyDown";
	}

	protected override bool ByPassLimit(UnitController current)
	{
		return current.unit.Status == UnitStateId.STUNNED;
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
