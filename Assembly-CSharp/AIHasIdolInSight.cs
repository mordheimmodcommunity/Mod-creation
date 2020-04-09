using RAIN.Core;

public class AIHasIdolInSight : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasIdolInSight";
		success = unitCtrlr.HasIdolInSight();
	}
}
