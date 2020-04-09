using Prometheus;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SpellCurse : ICheapState
{
	private UnitController unitCtrlr;

	private bool hasCurse;

	private StringBuilder logBldr;

	private bool damageApplied;

	public SpellCurse(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
		logBldr = new StringBuilder();
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		logBldr.Length = 0;
		SkillId skillId = SkillId.NONE;
		string name = null;
		switch (unitCtrlr.currentSpellTypeId)
		{
		case SpellTypeId.ARCANE:
			hasCurse = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, unitCtrlr.unit.TzeentchsCurseRoll, AttributeId.TZEENTCHS_CURSE_ROLL, reverse: true);
			name = "action/curse_tzeentch";
			break;
		case SpellTypeId.DIVINE:
			hasCurse = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, unitCtrlr.unit.DivineWrathRoll, AttributeId.DIVINE_WRATH_ROLL, reverse: true);
			name = "action/curse_divine_wrath";
			break;
		case SpellTypeId.WYRDSTONE:
			hasCurse = !unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, unitCtrlr.unit.WyrdstoneResistRoll + unitCtrlr.wyrdstoneRollModifier, AttributeId.WYRDSTONE_RESIST_ROLL);
			name = "action/curse_wyrdstone";
			break;
		case SpellTypeId.MISSION:
		{
			hasCurse = true;
			skillId = unitCtrlr.currentCurseSkillId;
			SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skillId);
			name = ((skillData.EffectTypeId != EffectTypeId.BUFF) ? "action/curse_debuff" : "action/curse_buff");
			break;
		}
		}
		unitCtrlr.defenderCtrlr = null;
		if (!hasCurse)
		{
			return;
		}
		if ((unitCtrlr.IsPlayed() && unitCtrlr.currentSpellTypeId == SpellTypeId.ARCANE) || unitCtrlr.currentSpellTypeId == SpellTypeId.DIVINE)
		{
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.SPELLS_CURSES, 1);
		}
		if (skillId == SkillId.NONE)
		{
			List<SpellCurseData> datas = PandoraSingleton<DataFactory>.Instance.InitData<SpellCurseData>("fk_spell_type_id", ((int)unitCtrlr.currentSpellTypeId).ToConstantString());
			Dictionary<SpellCurseId, int> curseModifiers = unitCtrlr.unit.GetCurseModifiers();
			skillId = (SpellCurseData.GetRandomRatio(datas, PandoraSingleton<MissionManager>.Instance.NetworkTyche, curseModifiers)?.SkillId ?? SkillId.NONE);
		}
		if (skillId != 0)
		{
			unitCtrlr.SetCurrentAction(skillId);
			unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction.LocalizedName, PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>(name, cached: true));
			SkillData skillData2 = unitCtrlr.CurrentAction.skillData;
			if (skillData2.TargetingId == TargetingId.NONE)
			{
				if (skillData2.TargetSelf && !skillData2.TargetAlly && !skillData2.TargetEnemy)
				{
					unitCtrlr.defenderCtrlr = unitCtrlr;
				}
			}
			else
			{
				unitCtrlr.UpdateTargetsData();
				unitCtrlr.CurrentAction.SetTargets();
				Vector3 sphereRaySrc;
				Vector3 sphereDir;
				switch (skillData2.TargetingId)
				{
				case TargetingId.SINGLE_TARGET:
					unitCtrlr.defenders = unitCtrlr.CurrentAction.Targets;
					break;
				case TargetingId.LINE:
					PandoraSingleton<MissionManager>.Instance.InitLineTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.RangeMax, out sphereRaySrc, out sphereDir);
					unitCtrlr.SetLineTargets(unitCtrlr.GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.lineTarget.transform, highlighTargets: false);
					break;
				case TargetingId.CONE:
					PandoraSingleton<MissionManager>.Instance.InitConeTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.RangeMax, out sphereRaySrc, out sphereDir);
					unitCtrlr.SetConeTargets(unitCtrlr.GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.coneTarget.transform, highlighTargets: false);
					break;
				case TargetingId.AREA:
					PandoraSingleton<MissionManager>.Instance.InitSphereTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.TargetingId, out sphereRaySrc, out sphereDir);
					unitCtrlr.SetAoeTargets(unitCtrlr.GetCurrentActionTargets(), PandoraSingleton<MissionManager>.Instance.sphereTarget.transform, highlightTargets: false);
					break;
				}
			}
			damageApplied = false;
			for (int i = 0; i < unitCtrlr.defenders.Count; i++)
			{
				UnitController unitController = unitCtrlr.defenders[i];
				logBldr.Append(unitController.GetLogName());
				if (i < unitCtrlr.defenders.Count - 1)
				{
					logBldr.Append(",");
				}
				PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitController.transform);
				unitController.attackerCtrlr = unitCtrlr;
				unitController.flyingLabel = skillData2.EffectTypeId.ToString();
				if (skillData2.WoundMax > 0)
				{
					int damage = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(unitCtrlr.CurrentAction.GetMinDamage(), unitCtrlr.CurrentAction.GetMaxDamage() + 1);
					unitController.ComputeDirectWound(damage, skillData2.BypassArmor, null);
					damageApplied = true;
				}
			}
			PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(unitCtrlr, CombatLogger.LogMessage.CURSE_TARGET, unitCtrlr.CurrentAction.LocalizedName, logBldr.ToString());
			string text = unitCtrlr.CurrentAction.fxData.SequenceId.ToLowerString();
			PandoraDebug.LogInfo("loading sequence " + text + " for curse " + skillId);
			PandoraSingleton<MissionManager>.Instance.PlaySequence(text, unitCtrlr, OnSeqDone);
			PandoraSingleton<MissionManager>.Instance.StartCoroutine(PlayDefense());
		}
		else
		{
			hasCurse = false;
		}
	}

	private IEnumerator PlayDefense()
	{
		yield return new WaitForSeconds(0.9f);
		for (int i = 0; i < unitCtrlr.defenders.Count; i++)
		{
			unitCtrlr.defenders[i].PlayDefState(unitCtrlr.defenders[i].attackResultId, (unitCtrlr.defenders[i].unit.Status == UnitStateId.OUT_OF_ACTION) ? 1 : 0, unitCtrlr.defenders[i].unit.Status);
		}
	}

	public void Exit(int iTo)
	{
	}

	public void Update()
	{
		if (!hasCurse)
		{
			OnSeqDone();
		}
	}

	public void FixedUpdate()
	{
	}

	private void OnSeqDone()
	{
		for (int i = 0; i < unitCtrlr.defenders.Count; i++)
		{
			unitCtrlr.defenders[i].EndDefense();
		}
		unitCtrlr.StateMachine.ChangeState(10);
		if (unitCtrlr.currentSpellSuccess && unitCtrlr.currentSpellId != 0)
		{
			ZoneAoe.Spawn(unitCtrlr, unitCtrlr.GetAction(unitCtrlr.currentSpellId));
			unitCtrlr.currentSpellId = SkillId.NONE;
		}
	}

	public void LaunchFx()
	{
		PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitCtrlr.CurrentAction.fxData.LaunchFx, unitCtrlr, null, null);
		unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_SPELL_CURSE);
		unitCtrlr.HitDefenders(unitCtrlr.transform.forward, damageApplied);
	}
}
