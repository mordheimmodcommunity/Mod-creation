using RAIN.Core;

public class AIHasBlackList : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasBlackList";
		success = (unitCtrlr.GetWarband().BlackList != 0);
	}
}
