using RAIN.Core;
using RAIN.Memory;
using RAIN.Representation;

public class AIIsEngagedParam : AIBase
{
	public Expression count;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "IsEngaged";
		if (unitCtrlr.Engaged)
		{
			unitCtrlr.AICtrlr.atDestination = false;
		}
		success = (unitCtrlr.EngagedUnits.Count >= count.Evaluate<int>(0f, (RAINMemory)null));
	}
}
