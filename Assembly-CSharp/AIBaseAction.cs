using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBaseAction : AIBase
{
	private ActionStatus bestAction;

	private UnitController refTarget;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "AIBaseAction";
		bestAction = unitCtrlr.AICtrlr.GetBestAction(GetRelatedActions(), out refTarget, RefineTargets);
		success = (bestAction != null && refTarget != null);
	}

	public override ActionResult Execute(AI ai)
	{
		if (success)
		{
			if (AIController.consSkillActions.Contains(bestAction.skillData.UnitActionId, UnitActionIdComparer.Instance))
			{
				unitCtrlr.AICtrlr.hasCastSkill = true;
			}
			unitCtrlr.FaceTarget(refTarget.transform, force: true);
			Vector3 lineSrc;
			Vector3 lineDir;
			switch (bestAction.TargetingId)
			{
			case TargetingId.AREA:
			case TargetingId.AREA_GROUND:
			{
				PandoraSingleton<MissionManager>.Instance.InitSphereTarget(unitCtrlr.transform, bestAction.Radius, bestAction.TargetingId, out lineSrc, out lineDir);
				PandoraSingleton<MissionManager>.Instance.sphereTarget.SetActive(value: false);
				Vector3 vector = refTarget.transform.position;
				if (bestAction.skillData.NeedValidGround)
				{
					if (refTarget == unitCtrlr)
					{
						vector += unitCtrlr.transform.forward * PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 2);
					}
					vector = PandoraSingleton<MissionManager>.Instance.ClampToNavMesh(vector);
				}
				unitCtrlr.SendSkillTargets(bestAction.SkillId, vector, vector - unitCtrlr.transform.position);
				break;
			}
			case TargetingId.CONE:
				PandoraSingleton<MissionManager>.Instance.InitConeTarget(unitCtrlr.transform, bestAction.Radius, bestAction.RangeMax, out lineSrc, out lineDir);
				PandoraSingleton<MissionManager>.Instance.coneTarget.SetActive(value: false);
				unitCtrlr.SendSkillTargets(bestAction.SkillId, lineSrc, lineDir);
				break;
			case TargetingId.LINE:
				PandoraSingleton<MissionManager>.Instance.InitLineTarget(unitCtrlr.transform, bestAction.Radius, bestAction.RangeMax, out lineSrc, out lineDir);
				PandoraSingleton<MissionManager>.Instance.lineTarget.SetActive(value: false);
				unitCtrlr.SendSkillTargets(bestAction.SkillId, lineSrc, lineDir);
				break;
			case TargetingId.SINGLE_TARGET:
				unitCtrlr.SendSkillSingleTarget(bestAction.SkillId, refTarget);
				break;
			case TargetingId.ARC:
				unitCtrlr.SendSkillTargets(bestAction.SkillId, unitCtrlr.transform.position, refTarget.transform.position - unitCtrlr.transform.position);
				break;
			}
			return (ActionResult)0;
		}
		return (ActionResult)2;
	}

	private void RefineTargets(ActionStatus action, List<UnitController> allTargets)
	{
		List<UnitController> list = new List<UnitController>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < allTargets.Count; i++)
		{
			if (!IsValid(action, allTargets[i]))
			{
				continue;
			}
			int criteriaValue = GetCriteriaValue(allTargets[i]);
			if (ByPassLimit(allTargets[i]) || list.Count < 3)
			{
				list.Add(allTargets[i]);
				list2.Add(criteriaValue);
				continue;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (IsBetter(criteriaValue, list2[num]))
				{
					list.RemoveAt(num);
					list2.RemoveAt(num);
					list.Add(allTargets[i]);
					list2.Add(criteriaValue);
					break;
				}
			}
		}
		allTargets.Clear();
		allTargets.AddRange(list);
	}

	protected void RemoveRandomTargets<U>(List<U> units)
	{
		while (units.Count > 3)
		{
			int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, units.Count);
			units.RemoveAt(index);
		}
	}

	protected abstract bool IsValid(ActionStatus action, UnitController target);

	protected abstract bool ByPassLimit(UnitController target);

	protected abstract bool IsBetter(int currentVal, int val);

	protected abstract int GetCriteriaValue(UnitController target);

	protected abstract List<UnitActionId> GetRelatedActions();
}
