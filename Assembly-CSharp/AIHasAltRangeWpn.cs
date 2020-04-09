using RAIN.Core;

public class AIHasAltRangeWpn : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasAltRangeWpn";
		success = unitCtrlr.IsAltRange();
	}
}
