using RAIN.Action;
using RAIN.Core;

public class AIEndTurn : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "EndTurn";
	}

	public override ActionResult Execute(AI ai)
	{
		unitCtrlr.SendSkill(SkillId.BASE_END_TURN);
		return (ActionResult)0;
	}
}
