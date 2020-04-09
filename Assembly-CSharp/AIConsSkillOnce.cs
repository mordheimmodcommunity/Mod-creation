using RAIN.Core;
using System.Collections.Generic;

public class AIConsSkillOnce : AIConsSkillSpell
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AIConsSkillOnce";
	}

	protected override bool IsValid(ActionStatus action, UnitController target)
	{
		unitCtrlr.AICtrlr.preFight = true;
		return !unitCtrlr.AICtrlr.hasCastSkill && base.IsValid(action, target);
	}

	protected override List<UnitActionId> GetRelatedActions()
	{
		return AIController.consSkillActions;
	}

	public override void Stop(AI ai)
	{
		base.Stop(ai);
		unitCtrlr.AICtrlr.preFight = false;
	}
}
