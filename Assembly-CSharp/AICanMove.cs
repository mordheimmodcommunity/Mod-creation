using RAIN.Core;

public class AICanMove : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "CanMove";
		success = (unitCtrlr.unit.CurrentStrategyPoints > 0);
	}
}
