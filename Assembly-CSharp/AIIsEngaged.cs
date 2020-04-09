using RAIN.Core;

public class AIIsEngaged : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "IsEngaged";
		if (unitCtrlr.Engaged)
		{
			unitCtrlr.AICtrlr.atDestination = false;
		}
		success = unitCtrlr.Engaged;
	}
}
