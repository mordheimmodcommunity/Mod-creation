using RAIN.Action;
using RAIN.Core;

public class AISwitchToBaseState : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "SwitchToBaseState";
	}

	public override ActionResult Execute(AI ai)
	{
		unitCtrlr.AICtrlr.GotoBaseMode();
		return (ActionResult)0;
	}
}
