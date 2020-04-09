using RAIN.Core;

public class AIHasBetterParry : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasBetterParry";
		success = (unitCtrlr.unit.ParryingRoll >= unitCtrlr.unit.DodgeRoll);
	}
}
