using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class SpellCasting : ICheapState
{
	private const float CLOSE_DISTANCE = 20f;

	private UnitController unitCtrlr;

	private GameObject projectileFx;

	private bool nextFrameHit;

	private UnitController defaultTarget;

	private List<UnitController> copyDefenders = new List<UnitController>();

	private bool damageApplied;

	public bool spellSuccess;

	public SpellCasting(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		damageApplied = false;
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.FaceTarget(unitCtrlr.currentSpellTargetPosition);
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		int roll = unitCtrlr.CurrentAction.GetRoll();
		if (unitCtrlr.CurrentAction.skillData.AutoSuccess)
		{
			spellSuccess = true;
		}
		else
		{
			spellSuccess = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, roll, AttributeId.SPELLCASTING_ROLL);
		}
		unitCtrlr.currentActionData.SetActionOutcome(spellSuccess);
		if (spellSuccess)
		{
			defaultTarget = unitCtrlr.defenderCtrlr;
			for (int num = unitCtrlr.defenders.Count - 1; num >= 0; num--)
			{
				UnitController unitController = unitCtrlr.defenders[num];
				unitController.beenShot = true;
				PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitController.transform);
				unitController.attackerCtrlr = unitCtrlr;
				unitController.flyingLabel = string.Empty;
				bool flag = false;
				if (!unitCtrlr.CurrentAction.skillData.BypassMagicResist)
				{
					int target = unitController.unit.MagicResistance + unitCtrlr.unit.MagicResistDefenderModifier;
					flag = unitController.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, target, AttributeId.MAGIC_RESISTANCE);
					PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitController, (!flag) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail_param", "#retro_outcome_resist") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_resist"), flag, (!flag) ? string.Empty : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_resist"));
				}
				if (!flag)
				{
					unitController.flyingLabel = unitCtrlr.CurrentAction.skillData.EffectTypeId.ToString();
					unitController.attackResultId = AttackResultId.NONE;
					if (unitCtrlr.CurrentAction.skillData.WoundMax > 0)
					{
						int damage = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(unitCtrlr.CurrentAction.GetMinDamage(), unitCtrlr.CurrentAction.GetMaxDamage() + 1);
						unitController.ComputeDirectWound(damage, unitCtrlr.CurrentAction.skillData.BypassArmor, unitCtrlr);
						damageApplied = true;
					}
				}
				else
				{
					unitCtrlr.defenders.RemoveAt(num);
				}
			}
			for (int i = 0; i < unitCtrlr.destructTargets.Count; i++)
			{
				unitCtrlr.ComputeDestructibleWound(unitCtrlr.destructTargets[i]);
			}
		}
		string text = PandoraSingleton<DataFactory>.Instance.InitData<SequenceData>((int)unitCtrlr.CurrentAction.fxData.SequenceId).Name;
		if (!spellSuccess)
		{
			text += "_fizzle";
		}
		else if (unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION)
		{
			text += "_ooa";
		}
		PandoraSingleton<MissionManager>.Instance.PlaySequence(text, unitCtrlr, OnSeqDone);
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
		if (nextFrameHit)
		{
			nextFrameHit = false;
			ProjectileHit();
		}
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void OnSeqDone()
	{
		if (spellSuccess && unitCtrlr.IsPlayed())
		{
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.SPELLS_CAST, 1);
		}
		for (int i = 0; i < unitCtrlr.defenders.Count; i++)
		{
			unitCtrlr.defenders[i].EndDefense();
		}
		copyDefenders.Clear();
		copyDefenders.AddRange(unitCtrlr.defenders);
		for (int j = 0; j < copyDefenders.Count; j++)
		{
			if (copyDefenders[j].Engaged)
			{
				copyDefenders[j].defenders.Clear();
				copyDefenders[j].defenders.AddRange(copyDefenders[j].EngagedUnits);
			}
			else
			{
				copyDefenders[j].defenders.Clear();
			}
			copyDefenders[j].TriggerEnchantments(EnchantmentTriggerId.ON_SPELL_RECEIVED);
		}
		unitCtrlr.currentSpellTypeId = unitCtrlr.CurrentAction.skillData.SpellTypeId;
		unitCtrlr.currentSpellId = unitCtrlr.CurrentAction.SkillId;
		unitCtrlr.currentSpellSuccess = spellSuccess;
		if (unitCtrlr.CurrentAction.skillData.AutoSuccess)
		{
			unitCtrlr.StateMachine.ChangeState(10);
		}
		else if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			unitCtrlr.SendCurse();
		}
		else
		{
			unitCtrlr.StateMachine.ChangeState(9);
		}
	}

	public void ShootProjectile()
	{
		if (spellSuccess)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.LaunchFx, unitCtrlr, null, null);
			UnitController unitController = unitCtrlr;
			UnitController endUnitCtrlr = null;
			if (unitCtrlr.CurrentAction.TargetingId == TargetingId.SINGLE_TARGET)
			{
				UnitController unitController2 = (!(unitCtrlr.defenderCtrlr != null)) ? defaultTarget : unitCtrlr.defenderCtrlr;
				if (unitController2 != null)
				{
					if (unitCtrlr.CurrentAction.fxData.ProjFromTarget)
					{
						unitController = unitController2;
					}
					endUnitCtrlr = unitController2;
				}
			}
			Vector3 startPos = (unitCtrlr.CurrentAction.fxData.SequenceId != SequenceId.SPELL_POINT) ? (-unitCtrlr.transform.position) : (-(unitCtrlr.BonesTr[BoneId.RIG_LARMPALM].position + unitCtrlr.BonesTr[BoneId.RIG_RARMPALM].position) / 2f);
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ProjectileFx, unitController, endUnitCtrlr, delegate(GameObject fx)
			{
				projectileFx = fx;
				if (projectileFx == null)
				{
					nextFrameHit = true;
				}
			}, startPos, unitCtrlr.currentSpellTargetPosition, ProjectileHit);
		}
		else
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.FizzleFx, unitCtrlr, null, delegate(GameObject fx)
			{
				if (fx != null && unitCtrlr.CurrentAction.fxData.SequenceId == SequenceId.SPELL_POINT)
				{
					fx.transform.LookAt(unitCtrlr.currentSpellTargetPosition);
				}
			});
		}
	}

	private void ProjectileHit()
	{
		if (spellSuccess)
		{
			unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SPELL_IMPACT);
			unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SPELL_IMPACT_RANDOM);
			if (damageApplied)
			{
				unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SPELL_IMPACT_DMG);
			}
			for (int i = 0; i < unitCtrlr.defenders.Count; i++)
			{
				unitCtrlr.defenders[i].TriggerAlliesEnchantments();
			}
		}
		Vector3 projDir = (!(projectileFx != null)) ? unitCtrlr.transform.forward : projectileFx.transform.forward;
		projDir.y = 0f;
		if (unitCtrlr.destructibleTarget != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ImpactFx, unitCtrlr.destructibleTarget.transform, attached: false, SetImpactFx);
		}
		else if (unitCtrlr.defenderCtrlr != null)
		{
			UnitController defenderCtrlr = unitCtrlr;
			if (unitCtrlr.CurrentAction.TargetingId == TargetingId.SINGLE_TARGET)
			{
				if (unitCtrlr.defenders.Count > 0)
				{
					defenderCtrlr = unitCtrlr.defenderCtrlr;
				}
				else if (defaultTarget != null)
				{
					defenderCtrlr = defaultTarget;
				}
			}
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ImpactFx, defenderCtrlr, null, SetImpactFx);
		}
		else
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.ImpactFx, null, null, SetImpactFx);
		}
		if (projectileFx != null)
		{
			Object.Destroy(projectileFx);
		}
		unitCtrlr.HitDefenders(projDir);
		for (int j = 0; j < unitCtrlr.destructTargets.Count; j++)
		{
			unitCtrlr.destructTargets[j].Hit(unitCtrlr);
		}
		if (unitCtrlr.CurrentAction.ZoneAoeId != 0 && unitCtrlr.CurrentAction.skillData.AutoSuccess)
		{
			ZoneAoe.Spawn(unitCtrlr, unitCtrlr.CurrentAction);
		}
		if (spellSuccess && unitCtrlr.CurrentAction.skillData.DestructibleId != 0)
		{
			Destructible.Spawn(unitCtrlr.CurrentAction.skillData.DestructibleId, unitCtrlr, unitCtrlr.currentSpellTargetPosition);
		}
	}

	private void SetImpactFx(GameObject impactFx)
	{
		if (impactFx != null && unitCtrlr.CurrentAction.TargetingId != TargetingId.SINGLE_TARGET && unitCtrlr.CurrentAction.RangeMax > 0)
		{
			impactFx.transform.position = unitCtrlr.currentSpellTargetPosition;
			impactFx.transform.rotation = Quaternion.identity;
		}
	}
}
