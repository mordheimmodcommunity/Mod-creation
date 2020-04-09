using RAIN.Core;

public class AIHasSpottedEnemies : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AIHasSpottedEnemies";
		success = (unitCtrlr.GetWarband().SquadManager.GetSpottedEnemies().Count > 0 || unitCtrlr.Engaged || unitCtrlr.beenShot);
	}
}
