using RAIN.Core;

public class AIHasEnemyInSight : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasEnemyInSight";
		success = (unitCtrlr.HasEnemyInSight() || unitCtrlr.Engaged || unitCtrlr.AICtrlr.hasSeenEnemy);
	}
}
