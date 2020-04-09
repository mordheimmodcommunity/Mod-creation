using RAIN.Core;

public class AIShootWeakest : AIShootHealthiest
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "ShootWeakest";
	}

	protected override bool IsBetter(int currentVal, int val)
	{
		return currentVal < val;
	}
}
