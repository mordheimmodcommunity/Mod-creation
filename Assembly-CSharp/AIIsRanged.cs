using RAIN.Core;

public class AIIsRanged : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "IsRanged";
		success = false;
	}
}
