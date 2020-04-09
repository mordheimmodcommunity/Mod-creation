using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trapped : ICheapState
{
	private UnitController unitCtrlr;

	public Trapped(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetAnimSpeed(0f);
		unitCtrlr.SetFixed(fix: true);
		if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
		{
			unitCtrlr.ValidMove();
		}
		TrapEffectData effectData = ((Trap)unitCtrlr.activeTrigger).EffectData;
		int trapResistRoll = unitCtrlr.unit.TrapResistRoll;
		if (!unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, trapResistRoll, AttributeId.TRAP_RESIST_ROLL))
		{
			List<TrapEffectJoinEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<TrapEffectJoinEnchantmentData>("fk_trap_effect_id", effectData.Id.ToIntString());
			for (int i = 0; i < list.Count; i++)
			{
				Enchantment enchantment = unitCtrlr.unit.AddEnchantment(list[i].EnchantmentId, unitCtrlr.unit, original: false, updateAttributes: false);
				if (enchantment != null && !enchantment.Data.NoDisplay)
				{
					unitCtrlr.buffResultId = enchantment.Data.EffectTypeId;
					PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_ENCHANTMENT, unitCtrlr, enchantment.LocalizedName, enchantment.Data.EffectTypeId, string.Empty);
				}
			}
			int enchantmentDamage = unitCtrlr.unit.GetEnchantmentDamage(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_APPLY);
			if (enchantmentDamage > 0)
			{
				unitCtrlr.buffResultId = EffectTypeId.NONE;
				unitCtrlr.ComputeDirectWound(enchantmentDamage, byPassArmor: true, null);
			}
			unitCtrlr.unit.UpdateAttributes();
		}
		if (unitCtrlr.IsPlayed())
		{
			PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.TRAPS, 1);
		}
		unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("skill_name_", effectData.Name), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/trap", cached: true));
		PandoraSingleton<MissionManager>.Instance.PlaySequence("trap", unitCtrlr, OnSeqDone);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitCtrlr);
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

	public void OnSeqDone()
	{
		unitCtrlr.EndDefense();
		Object.Destroy(unitCtrlr.activeTrigger);
		PandoraSingleton<GameManager>.Instance.StartCoroutine(Wait());
		unitCtrlr.activeTrigger = null;
	}

	private IEnumerator Wait()
	{
		yield return new WaitForSeconds(0.75f);
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			unitCtrlr.SendStartMove(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
		}
		else
		{
			unitCtrlr.StateMachine.ChangeState(9);
		}
	}
}
