using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIMoveOnPathSpell : AIMoveOnPath
{
	private ActionStatus spell;

	private List<UnitController> targets;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "MoveOnPathSpell";
		List<ActionStatus> actions = unitCtrlr.GetActions(UnitActionId.SPELL);
		success &= (actions.Count > 0);
		if (success)
		{
			spell = actions[0];
		}
	}

	public override ActionResult Execute(AI ai)
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (spell == null)
		{
			return (ActionResult)2;
		}
		targets = spell.Targets;
		if (targets.Count > 0)
		{
			spell.UpdateAvailable();
			if (spell.Available)
			{
				switch (spell.skillData.TargetingId)
				{
				case TargetingId.SINGLE_TARGET:
					unitCtrlr.SendSkillSingleTarget(spell.SkillId, targets[0]);
					return (ActionResult)0;
				case TargetingId.AREA:
					unitCtrlr.SendSkillTargets(spell.SkillId, targets[0].transform.position, targets[0].transform.position - unitCtrlr.transform.position);
					return (ActionResult)0;
				case TargetingId.LINE:
				case TargetingId.CONE:
					unitCtrlr.SendSkillTargets(spell.SkillId, unitCtrlr.transform.position + Vector3.up, targets[0].transform.position - unitCtrlr.transform.position);
					return (ActionResult)0;
				}
			}
		}
		return base.Execute(ai);
	}
}
