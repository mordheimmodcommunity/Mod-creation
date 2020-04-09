using RAIN.Core;

public class AISwitchToAlternateState : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "SwitchToActiveState";
		success = unitCtrlr.AICtrlr.GotoAlternateMode();
	}
}
