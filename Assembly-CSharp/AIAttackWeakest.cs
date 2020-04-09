using RAIN.Core;

public class AIAttackWeakest : AIAttackHealthiest
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AttackWeakest";
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		return currentVal < val;
	}
}
