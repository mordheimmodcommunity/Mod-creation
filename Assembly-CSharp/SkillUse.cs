using Prometheus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUse : ICheapState
{
	private UnitController unitCtrlr;

	private GameObject projectileFx;

	private bool projectileShot;

	public SkillUse(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.currentActionData.Reset();
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		unitCtrlr.flyingLabel = string.Empty;
		unitCtrlr.searchVariation = 0;
		projectileShot = false;
		if (unitCtrlr.CurrentAction.LinkedItem != null && unitCtrlr.CurrentAction.LinkedItem.ConsumableData.ConsumeItem)
		{
			unitCtrlr.unit.RemoveItem(unitCtrlr.CurrentAction.LinkedItem);
		}
		string empty = string.Empty;
		if (unitCtrlr.CurrentAction.fxData != null && unitCtrlr.CurrentAction.fxData.SequenceId != 0)
		{
			empty = unitCtrlr.CurrentAction.fxData.SequenceId.ToLowerString();
		}
		else if (unitCtrlr.CurrentAction.ActionId == UnitActionId.CONSUMABLE)
		{
			empty = "consumable";
		}
		else
		{
			empty = "skill";
			unitCtrlr.StopCoroutine(ApplyVisualEffect());
			unitCtrlr.StartCoroutine(ApplyVisualEffect());
		}
		PandoraSingleton<MissionManager>.Instance.PlaySequence(empty, unitCtrlr, OnSeqDone);
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	private IEnumerator ApplyVisualEffect()
	{
		yield return new WaitForSeconds(0.5f);
		unitCtrlr.HitDefenders(unitCtrlr.transform.forward, hurt: false);
	}

	public void OnInteract()
	{
		if (unitCtrlr.CurrentAction.skillData.TrapTypeId != 0)
		{
			Trap.SpawnTrap(unitCtrlr.CurrentAction.skillData.TrapTypeId, unitCtrlr.GetWarband().teamIdx, unitCtrlr.transform.position, unitCtrlr.transform.rotation);
			unitCtrlr.LaunchAction(UnitActionId.SEARCH, success: true, UnitStateId.NONE, 1);
		}
	}

	public void ShootProjectile()
	{
		projectileShot = true;
		PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.LaunchFx, unitCtrlr, null, null);
		Vector3 position = unitCtrlr.BonesTr[BoneId.RIG_RARMPALM].position;
		PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ProjectileFx, null, null, delegate(GameObject fx)
		{
			projectileFx = fx;
		}, position, unitCtrlr.currentSpellTargetPosition, OnProjectileHit);
	}

	public void OnProjectileHit()
	{
		unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SKILL_IMPACT);
		unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SKILL_IMPACT_RANDOM);
		Object.Destroy(projectileFx);
		PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ImpactFx, null, null, delegate(GameObject fx)
		{
			if (fx != null)
			{
				fx.transform.position = unitCtrlr.currentSpellTargetPosition;
				fx.transform.rotation = Quaternion.identity;
			}
		});
		PandoraSingleton<SequenceManager>.Instance.EndSequence();
	}

	public void OnSeqDone()
	{
		if (unitCtrlr.CurrentAction.ActionId == UnitActionId.CONSUMABLE || projectileShot)
		{
			unitCtrlr.HitDefenders(Vector3.zero);
		}
		ZoneAoe.Spawn(unitCtrlr, unitCtrlr.CurrentAction);
		List<SkillPerformSkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<SkillPerformSkillData>("fk_skill_id", ((int)unitCtrlr.CurrentAction.SkillId).ToConstantString());
		SkillId skillId = SkillId.NONE;
		if (unitCtrlr.defenderCtrlr != null && list != null && list.Count > 0 && list[0].SkillIdPerformed != 0)
		{
			skillId = list[0].SkillIdPerformed;
			if (list[0].AttributeIdRoll != 0)
			{
				bool flag = unitCtrlr.defenderCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, list[0].AttributeIdRoll);
				if (list[0].Success != flag)
				{
					skillId = SkillId.NONE;
				}
			}
		}
		if (unitCtrlr.CurrentAction.skillData.DestructibleId != 0)
		{
			Destructible.Spawn(unitCtrlr.CurrentAction.skillData.DestructibleId, unitCtrlr, unitCtrlr.currentSpellTargetPosition);
		}
		if (skillId != 0)
		{
			ActionStatus action = unitCtrlr.defenderCtrlr.GetAction(skillId);
			switch (action.TargetingId)
			{
			case TargetingId.NONE:
				unitCtrlr.defenderCtrlr.SkillRPC((int)list[0].SkillIdPerformed);
				break;
			case TargetingId.SINGLE_TARGET:
				if (action.Targets.Count > 0)
				{
					int num = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, action.Targets.Count);
					if (unitCtrlr.CurrentAction.SkillId == SkillId.THREATEN || unitCtrlr.CurrentAction.SkillId == SkillId.THREATEN_MSTR)
					{
						int num2 = action.Targets.IndexOf(unitCtrlr);
						num = ((num2 == -1) ? num : num2);
					}
					unitCtrlr.defenderCtrlr.SkillSingleTargetRPC((int)skillId, action.Targets[num].uid);
				}
				else if (action.Destructibles.Count > 0)
				{
					int index = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, action.Destructibles.Count);
					unitCtrlr.defenderCtrlr.SendSkillSingleDestructible(skillId, action.Destructibles[index]);
				}
				break;
			}
			if (unitCtrlr != unitCtrlr.defenderCtrlr)
			{
				unitCtrlr.WaitForAction(UnitController.State.START_MOVE);
			}
		}
		else
		{
			unitCtrlr.StateMachine.ChangeState(10);
		}
	}
}
