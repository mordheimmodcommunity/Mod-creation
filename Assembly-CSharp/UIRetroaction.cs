using DG.Tweening;
using DG.Tweening.Core;
using System.Collections.Generic;
using UnityEngine;

public class UIRetroaction : MonoBehaviour
{
	public UIRetroactionAction playerUnitAction;

	public UIRetroactionAction enemyUnitAction;

	public List<UIRetroactionTarget> playerTargets;

	public List<UIRetroactionTarget> enemyTargets;

	private readonly List<RetroactionInfo> targetsInfo = new List<RetroactionInfo>();

	private bool actionSet;

	private bool clear;

	public UIRetroactionAction RetroactionAction => (!PandoraSingleton<MissionManager>.Instance.focusedUnit.IsPlayed()) ? enemyUnitAction : playerUnitAction;

	public int Dir => PandoraSingleton<MissionManager>.Instance.focusedUnit.IsPlayed() ? 1 : (-1);

	private void Awake()
	{
		for (int i = 0; i < playerTargets.Count; i++)
		{
			RetroactionInfo retroactionInfo = new RetroactionInfo();
			retroactionInfo.playerUnitAction = playerTargets[i];
			retroactionInfo.enemyUnitAction = enemyTargets[i];
			targetsInfo.Add(retroactionInfo);
		}
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SEQUENCE_STARTED, StartSequence);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SEQUENCE_ENDED, EndSequence);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_ACTION, SetAction);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_ACTION_CLEAR, EndSequence);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_ACTION_OUTCOME, SetActionOutcome);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_SHOW_OUTCOME, ShowActionOutcome);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_TARGET_STATUS, ShowTargetStatus);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_TARGET_DAMAGE, SetTargetDamage);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_TARGET_OUTCOME, SetTargetOutcome);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.RETROACTION_TARGET_ENCHANTMENT, SetTargetEnchantment);
	}

	private void ShowTargetStatus()
	{
		object target = PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		string status = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		GetTarget(target)?.ShowStatus(status);
	}

	private void Start()
	{
		EndSequence();
	}

	private void ShowActionOutcome()
	{
		ShowActionOutcome(0f);
	}

	private void ShowActionOutcome(float delay)
	{
		object target = PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		RetroactionInfo target2 = GetTarget(target);
		if (target2 == null || !actionSet)
		{
			return;
		}
		target2.ShowOutcome();
		target2.ShowEnchant();
		target2.RetroactionTarget.check.Show();
		DoMove(target2.RetroactionTarget.check.offset, 600f * (float)Dir, 0f);
		for (int i = 0; i < target2.RetroactionTarget.enchants.Count; i++)
		{
			if (!string.IsNullOrEmpty(target2.RetroactionTarget.enchants[i].resultName.get_text()))
			{
				target2.RetroactionTarget.enchants[i].Show();
				DoMove(target2.RetroactionTarget.enchants[i].offset, -600f * (float)Dir, delay + 0.35f + (float)i * 0.1f);
			}
		}
	}

	public void StartSequence()
	{
	}

	public void EndSequence()
	{
		clear = false;
		playerUnitAction.gameObject.SetActive(value: false);
		enemyUnitAction.gameObject.SetActive(value: false);
		for (int i = 0; i < targetsInfo.Count; i++)
		{
			targetsInfo[i].Reset();
		}
		actionSet = false;
	}

	private void SetAction()
	{
		UnitController unitCtrlr = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		if (!IsTargetValid(unitCtrlr))
		{
			return;
		}
		actionSet = true;
		UIRetroactionAction retroactionAction = RetroactionAction;
		retroactionAction.gameObject.SetActive(value: true);
		retroactionAction.Set(unitCtrlr);
		DoMove(retroactionAction.offset, -800f * (float)Dir, 0f);
		for (int i = 0; i < targetsInfo.Count; i++)
		{
			if (targetsInfo[i].unitCtrlr != null || targetsInfo[i].destructible != null)
			{
				targetsInfo[i].ShowTarget();
				DoMove(targetsInfo[i].RetroactionTarget.offset, -600f * (float)Dir, 0.5f + 0.2f * (float)i);
			}
		}
		ShowActionOutcome(0.5f);
	}

	private void DoMove(RectTransform rectTransform, float posX, float delay)
	{
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = posX;
		rectTransform.anchoredPosition = anchoredPosition;
		TweenSettingsExtensions.SetDelay<Tweener>(TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector2>)(() => rectTransform.anchoredPosition), (DOSetter<Vector2>)delegate(Vector2 x)
		{
			rectTransform.anchoredPosition = x;
		}, Vector2.zero, 0.15f), (AxisConstraint)2, true), (object)rectTransform), delay);
	}

	private void SetActionOutcome()
	{
		UnitController unitCtrlr = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		if (IsTargetValid(unitCtrlr))
		{
			RetroactionAction.result.Set(unitCtrlr);
		}
	}

	private void SetTargetOutcome()
	{
		object target = PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		string actionEffect = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		bool success = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
		string damageEffect = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[3];
		TryAddTarget(target)?.AddOutcome(actionEffect, success, damageEffect);
	}

	private void SetTargetDamage()
	{
		object target = PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		int damage = (int)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		bool critical = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
		RetroactionInfo retroactionInfo = TryAddTarget(target);
		if (retroactionInfo == null)
		{
			retroactionInfo = GetTarget(target);
		}
		retroactionInfo?.AddDamage(damage, critical);
	}

	private void SetTargetEnchantment()
	{
		if (PandoraSingleton<NoticeManager>.Instance.Parameters[0] is Unit)
		{
			Unit unit = (Unit)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
			string enchantmentId = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
			EffectTypeId effectTypeId = (EffectTypeId)(int)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
			GetTarget(unit)?.AddEnchant(enchantmentId, effectTypeId, string.Empty);
		}
		else if (PandoraSingleton<NoticeManager>.Instance.Parameters[0] is UnitController)
		{
			UnitController unitCtrlr = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
			string enchantmentId2 = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
			EffectTypeId effectTypeId2 = (EffectTypeId)(int)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
			string effect = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[3];
			TryAddTarget(unitCtrlr)?.AddEnchant(enchantmentId2, effectTypeId2, effect);
		}
	}

	private RetroactionInfo TryAddTarget(object target)
	{
		if (!clear)
		{
			EndSequence();
			clear = true;
		}
		UnitController unitController = target as UnitController;
		if (unitController != null)
		{
			return TryAddTarget(unitController);
		}
		Destructible destructible = target as Destructible;
		if (destructible != null)
		{
			return TryAddTarget(destructible);
		}
		return null;
	}

	private RetroactionInfo TryAddTarget(UnitController unitCtrlr)
	{
		if (!IsTargetValid(unitCtrlr))
		{
			return null;
		}
		RetroactionInfo retroactionInfo = GetTarget(unitCtrlr);
		if (retroactionInfo == null)
		{
			int index = targetsInfo.FindIndex((RetroactionInfo x) => x.unitCtrlr == null && x.destructible == null);
			retroactionInfo = targetsInfo[index];
			retroactionInfo.SetTarget(unitCtrlr);
		}
		return retroactionInfo;
	}

	private RetroactionInfo TryAddTarget(Destructible dest)
	{
		if (!IsTargetValid(dest))
		{
			return null;
		}
		RetroactionInfo retroactionInfo = GetTarget(dest);
		if (retroactionInfo == null)
		{
			int index = targetsInfo.FindIndex((RetroactionInfo x) => x.unitCtrlr == null && x.destructible == null);
			retroactionInfo = targetsInfo[index];
			retroactionInfo.SetTarget(dest);
		}
		return retroactionInfo;
	}

	public bool IsTargetValid(UnitController unitCtrlr)
	{
		return unitCtrlr.Imprint.State == MapImprintStateId.VISIBLE;
	}

	public bool IsTargetValid(Destructible dest)
	{
		return dest.Imprint.State == MapImprintStateId.VISIBLE;
	}

	private RetroactionInfo GetTarget(object target)
	{
		UnitController unitController = target as UnitController;
		if (unitController != null)
		{
			return GetTarget(unitController);
		}
		Unit unit = target as Unit;
		if (unit != null)
		{
			return GetTarget(unit);
		}
		Destructible destructible = target as Destructible;
		if (destructible != null)
		{
			return GetTarget(destructible);
		}
		return null;
	}

	private RetroactionInfo GetTarget(UnitController unitCtrlr)
	{
		return targetsInfo.Find((RetroactionInfo x) => x.unitCtrlr == unitCtrlr);
	}

	private RetroactionInfo GetTarget(Unit unit)
	{
		return targetsInfo.Find((RetroactionInfo x) => x.unitCtrlr != null && x.unitCtrlr.unit == unit);
	}

	private RetroactionInfo GetTarget(Destructible dest)
	{
		return targetsInfo.Find((RetroactionInfo x) => x.destructible == dest);
	}
}
