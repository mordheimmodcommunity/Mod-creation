using RAIN.Action;
using RAIN.Core;

public class AIStanceDodge : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "StanceDodge";
		success = unitCtrlr.GetAction(SkillId.BASE_STANCE_DODGE).Available;
	}

	public override ActionResult Execute(AI ai)
	{
		if (success)
		{
			unitCtrlr.SendSkill(SkillId.BASE_STANCE_DODGE);
			return (ActionResult)0;
		}
		return (ActionResult)2;
	}
}
