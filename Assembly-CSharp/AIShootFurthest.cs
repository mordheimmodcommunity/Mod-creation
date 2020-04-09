using RAIN.Core;

public class AIShootFurthest : AIShootClosest
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "ShootFurthest";
	}

	protected override bool IsBetter(int currentDist, int dist)
	{
		return currentDist > dist;
	}
}
