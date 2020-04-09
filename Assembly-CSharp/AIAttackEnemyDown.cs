using RAIN.Core;

public class AIAttackEnemyDown : AIAttackBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AttackEnemyDown";
	}

	protected override bool IsValid(ActionStatus action, UnitController target)
	{
		return target.unit.Status == UnitStateId.STUNNED && base.IsValid(action, target);
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
}
