using RAIN.Action;
using RAIN.Core;

public class AIStanceAmbush : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "StanceAmbush";
		success = unitCtrlr.GetAction(SkillId.BASE_STANCE_AMBUSH).Available;
	}

	public override ActionResult Execute(AI ai)
	{
		if (success)
		{
			unitCtrlr.unit.SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, 1);
			if (unitCtrlr.GetAction(SkillId.BASE_JUMPDOWN).Available)
			{
				unitCtrlr.SendSkill(SkillId.BASE_JUMPDOWN);
			}
			else
			{
				unitCtrlr.SendSkill(SkillId.BASE_STANCE_AMBUSH);
			}
			return (ActionResult)0;
		}
		return (ActionResult)2;
	}
}
