using RAIN.Core;

public class AIHasAltMeleeWpn : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasAltMeleeWpn";
		success = unitCtrlr.IsAltClose();
	}
}
