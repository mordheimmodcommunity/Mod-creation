using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCurrentActionController : UIUnitControllerChanged
{
	public Text percent;

	public Text damage;

	public Image action;

	public Image mastery;

	public Image prevAction;

	public Image prevMastery;

	public Image nextAction;

	public Image nextMastery;

	public Text actionName;

	public GameObject noAction;

	public Text noActionText;

	public List<GameObject> offensePoints;

	public List<GameObject> strategyPoints;

	private ActionStatus currentAction;

	public GameObject navigation;

	public ImageGroup leftCycling;

	public ImageGroup rightCycling;

	public ButtonGroup confirm;

	public ButtonGroup cancel;

	public GameObject leftMoreInfoActionGo;

	public GameObject rightMoreInfoActionGo;

	public List<UIExtraActionInfo> rollExtraInfo;

	public List<UIExtraActionInfo> damageExtraInfo;

	private ActionStatus currentActionParameter;

	private ActionStatus prevActionParameter;

	private ActionStatus nextActionParameter;

	private bool hasActionsParameter;

	private bool isActionsCountParameter;

	private bool isActionsCount1Parameter;

	protected virtual void Awake()
	{
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_ACTION_CHANGED, OnCurrentActionChanged);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_TARGET_CHANGED, OnCurrentTargetChanged);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_ACTION_CONFIRMATION_BOX, delegate
		{
			SetConfirmation(isCounter: false);
		});
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_ACTION_CANCEL, ActionCanceled);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_MORE_INFO_UNIT_ACTION_TOGGLE, UpdateMoreInfoVisibility);
	}

	private void OnCurrentTargetChanged()
	{
		UnitController target = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController;
		OnCurrentTargetChanged(target);
	}

	private void OnCurrentTargetChanged(UnitController target)
	{
		if (currentAction != null)
		{
			SetWarningMessage();
			UpdateRollAndDamage();
		}
	}

	private void Start()
	{
		confirm.SetAction("action", string.Empty);
		confirm.SetInteractable(inter: false);
		cancel.SetInteractable(inter: false);
		cancel.SetAction("cancel", "menu_cancel");
		leftCycling.SetAction("cycling", string.Empty);
		rightCycling.SetAction("cycling", string.Empty, 0, negative: true);
	}

	private void OnCurrentActionChanged()
	{
		if (PandoraSingleton<NoticeManager>.Instance.Parameters != null)
		{
			UnitController unitController = null;
			ActionStatus actionStatus = null;
			List<ActionStatus> list = null;
			if (PandoraSingleton<NoticeManager>.Instance.Parameters.Count >= 1)
			{
				unitController = (PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController);
			}
			if (unitController != null && unitController == base.CurrentUnitController && unitController.IsPlayed())
			{
				actionStatus = ((PandoraSingleton<NoticeManager>.Instance.Parameters.Count < 2) ? base.CurrentUnitController.CurrentAction : (PandoraSingleton<NoticeManager>.Instance.Parameters[1] as ActionStatus));
				list = ((PandoraSingleton<NoticeManager>.Instance.Parameters.Count < 3) ? base.CurrentUnitController.availableActionStatus : (PandoraSingleton<NoticeManager>.Instance.Parameters[2] as List<ActionStatus>));
				OnCurrentActionChanged(unitController, actionStatus, list);
			}
		}
	}

	private void OnCurrentActionChanged(UnitController controller, ActionStatus actionStatus, List<ActionStatus> actionsStatus)
	{
		if (base.CurrentUnitController != null && base.CurrentUnitController == controller && base.CurrentUnitController.IsPlayed())
		{
			base.UpdateUnit = false;
			currentActionParameter = actionStatus;
			nextActionParameter = null;
			prevActionParameter = null;
			hasActionsParameter = false;
			isActionsCountParameter = false;
			isActionsCount1Parameter = false;
			hasActionsParameter = (actionsStatus != null);
			isActionsCountParameter = (actionsStatus != null && actionsStatus.Count > 0);
			isActionsCount1Parameter = (actionsStatus != null && actionsStatus.Count == 1);
			if (actionsStatus != null && isActionsCountParameter)
			{
				int num = actionsStatus.IndexOf(currentActionParameter);
				nextActionParameter = actionsStatus[(num + 1 < actionsStatus.Count) ? (num + 1) : 0];
				prevActionParameter = actionsStatus[(num - 1 < 0) ? (actionsStatus.Count - 1) : (num - 1)];
			}
			else
			{
				nextActionParameter = (prevActionParameter = null);
			}
			CurrentActionChanged();
		}
	}

	private void ActionCanceled()
	{
		if (base.CurrentUnitController != null)
		{
			actionName.set_text(base.CurrentUnitController.CurrentAction.LocalizedName);
			cancel.gameObject.SetActive(value: false);
		}
	}

	public void SetConfirmation(bool isCounter)
	{
		if (base.CurrentUnitController != null && base.CurrentUnitController.CurrentAction != null)
		{
			actionName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_confirm_action", (!isCounter) ? base.CurrentUnitController.CurrentAction.LocalizedName : PandoraSingleton<LocalizationManager>.Instance.GetStringById("reaction_melee_attack")));
			cancel.gameObject.SetActive(value: true);
		}
	}

	private void CurrentActionChanged()
	{
		if (!(base.CurrentUnitController != null))
		{
			return;
		}
		bool flag = false;
		if (base.CurrentUnitController.IsPlayed() && base.TargetUnitController != null && !base.TargetUnitController.IsPlayed() && base.TargetUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE))
		{
			flag = true;
			currentAction = null;
		}
		else
		{
			currentAction = currentActionParameter;
		}
		if (currentAction != null)
		{
			action.set_overrideSprite(GetActionIcon());
			((Behaviour)(object)mastery).enabled = currentAction.IsMastery;
			UpdateRollAndDamage();
			SetWarningMessage();
			noAction.SetActive(value: false);
			confirm.gameObject.SetActive(value: true);
			cancel.gameObject.SetActive(currentAction.waitForConfirmation || base.CurrentUnitController.IsTargeting());
			if (currentAction.waitForConfirmation || base.CurrentUnitController.IsTargeting())
			{
				SetConfirmation(base.CurrentUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE));
			}
			else
			{
				actionName.set_text(currentAction.LocalizedName);
			}
			if (isActionsCountParameter)
			{
				navigation.SetActive(value: true);
				((Behaviour)(object)action).enabled = true;
				if (isActionsCount1Parameter || base.CurrentUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE))
				{
					navigation.SetActive(value: false);
					((Behaviour)(object)prevAction).enabled = false;
					((Behaviour)(object)prevMastery).enabled = false;
					((Behaviour)(object)nextAction).enabled = false;
					((Behaviour)(object)nextMastery).enabled = false;
				}
				else
				{
					navigation.SetActive(value: true);
					((Behaviour)(object)prevAction).enabled = true;
					((Behaviour)(object)nextAction).enabled = true;
					if (base.CurrentUnitController.IsCurrentState(UnitController.State.INTERACTIVE_TARGET))
					{
						prevAction.set_overrideSprite(base.CurrentUnitController.prevInteractiveTarget.action.GetIcon());
						((Behaviour)(object)prevMastery).enabled = false;
						nextAction.set_overrideSprite(base.CurrentUnitController.nextInteractiveTarget.action.GetIcon());
						((Behaviour)(object)nextMastery).enabled = false;
					}
					else
					{
						prevAction.set_overrideSprite(prevActionParameter.GetIcon());
						((Behaviour)(object)prevMastery).enabled = prevActionParameter.IsMastery;
						nextAction.set_overrideSprite(nextActionParameter.GetIcon());
						((Behaviour)(object)nextMastery).enabled = nextActionParameter.IsMastery;
					}
				}
			}
			else
			{
				if (base.CurrentUnitController.IsChoosingTarget())
				{
					navigation.SetActive(value: true);
				}
				else
				{
					navigation.SetActive(value: false);
				}
				((Behaviour)(object)prevAction).enabled = false;
				((Behaviour)(object)prevMastery).enabled = false;
				((Behaviour)(object)nextAction).enabled = false;
				((Behaviour)(object)nextMastery).enabled = false;
			}
			for (int i = 0; i < strategyPoints.Count; i++)
			{
				strategyPoints[i].SetActive(i < currentAction.StrategyPoints);
			}
			for (int j = 0; j < offensePoints.Count; j++)
			{
				offensePoints[j].SetActive(j < currentAction.OffensePoints);
			}
		}
		else
		{
			navigation.SetActive(value: false);
			confirm.gameObject.SetActive(value: false);
			cancel.gameObject.SetActive(value: false);
			((Behaviour)(object)damage).enabled = false;
			((Behaviour)(object)percent).enabled = false;
			if (flag)
			{
				noActionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("waiting_opponent"));
			}
			else
			{
				noActionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_none"));
			}
			noAction.SetActive(value: true);
			action.set_overrideSprite((Sprite)null);
			((Behaviour)(object)prevAction).enabled = false;
			((Behaviour)(object)prevMastery).enabled = false;
			((Behaviour)(object)nextAction).enabled = false;
			((Behaviour)(object)nextMastery).enabled = false;
			for (int k = 0; k < strategyPoints.Count; k++)
			{
				strategyPoints[k].SetActive(value: false);
			}
			for (int l = 0; l < offensePoints.Count; l++)
			{
				offensePoints[l].SetActive(value: false);
			}
			for (int m = 0; m < rollExtraInfo.Count; m++)
			{
				rollExtraInfo[m].gameObject.SetActive(value: false);
			}
			for (int n = 0; n < damageExtraInfo.Count; n++)
			{
				damageExtraInfo[n].gameObject.SetActive(value: false);
			}
		}
	}

	private void SetWarningMessage()
	{
		switch (currentAction.ActionId)
		{
		case UnitActionId.SPELL:
			if (base.CurrentUnitController.IsTargeting())
			{
				switch (currentAction.skillData.SpellTypeId)
				{
				case SpellTypeId.ARCANE:
					PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.WarningNoTimer("reaction_spell_arcane", base.CurrentUnitController.unit.TzeentchsCurseRoll);
					break;
				case SpellTypeId.DIVINE:
					PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.WarningNoTimer("reaction_spell_divine", base.CurrentUnitController.unit.DivineWrathRoll);
					break;
				}
			}
			else
			{
				PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.HideWithTimer();
			}
			break;
		case UnitActionId.MELEE_ATTACK:
		case UnitActionId.CHARGE:
		{
			UnitController target = currentAction.GetTarget();
			if (base.CurrentUnitController.IsTargeting() && target != null && target.CanCounterAttack() && !base.CurrentUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE))
			{
				PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.WarningNoTimer("reaction_melee_attack");
			}
			else
			{
				PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.HideWithTimer();
			}
			break;
		}
		case UnitActionId.LEAP:
		case UnitActionId.CLIMB:
		case UnitActionId.JUMP:
			if (base.CurrentUnitController.IsCurrentState(UnitController.State.INTERACTIVE_TARGET))
			{
				PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.WarningNoTimer("reaction_fall", -1, currentAction.GetMinDamage(), currentAction.GetMaxDamage());
			}
			else
			{
				PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.HideWithTimer();
			}
			break;
		case UnitActionId.FLEE:
			PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.WarningNoTimer("reaction_flee", -1, -1, -1, isPotential: false);
			break;
		default:
			PandoraSingleton<UIMissionManager>.Instance.rightSequenceMessage.HideWithTimer();
			break;
		}
	}

	private Sprite GetActionIcon()
	{
		if (base.CurrentUnitController != null && currentAction.IsInteractive && base.CurrentUnitController.interactivePoint != null && (base.CurrentUnitController.interactivePoint.unitActionId == UnitActionId.SEARCH || base.CurrentUnitController.interactivePoint.unitActionId == UnitActionId.ACTIVATE))
		{
			return base.CurrentUnitController.interactivePoint.GetIconAction();
		}
		return currentAction.GetIcon();
	}

	protected override void OnUnitChanged()
	{
		if ((!(base.CurrentUnitController != null) || !base.CurrentUnitController.IsPlayed()) && base.TargetUnitController != null && !base.TargetUnitController.IsPlayed())
		{
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (base.IsVisible)
		{
			UpdateRollAndDamage();
		}
	}

	private void UpdateRollAndDamage()
	{
		if (currentAction == null || !currentAction.Available)
		{
			return;
		}
		int roll = currentAction.GetRoll();
		if (roll != -1)
		{
			((Behaviour)(object)percent).enabled = true;
			percent.set_text(roll.ToConstantPercString());
		}
		else
		{
			((Behaviour)(object)percent).enabled = false;
		}
		if (currentAction.ActionId != UnitActionId.CLIMB && currentAction.ActionId != UnitActionId.JUMP && currentAction.ActionId != UnitActionId.LEAP)
		{
			base.CurrentUnitController.RecalculateModifiers();
			if (!currentAction.HasDamage())
			{
				((Behaviour)(object)damage).enabled = false;
			}
			else
			{
				((Behaviour)(object)damage).enabled = true;
				damage.set_text(PandoraUtils.StringBuilder.Append(currentAction.GetMinDamage().ToConstantString()).Append('-').Append(currentAction.GetMaxDamage().ToConstantString())
					.ToString());
			}
		}
		else
		{
			((Behaviour)(object)damage).enabled = false;
		}
		UpdateMoreInfoVisibility();
		for (int i = 0; i < rollExtraInfo.Count; i++)
		{
			if (i < base.CurrentUnitController.CurrentRollModifiers.Count)
			{
				rollExtraInfo[i].gameObject.SetActive(value: true);
				rollExtraInfo[i].text.set_text(base.CurrentUnitController.CurrentRollModifiers[i].GetText(forcePercent: true));
			}
			else
			{
				rollExtraInfo[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < damageExtraInfo.Count; j++)
		{
			if (j < base.CurrentUnitController.CurrentDamageModifiers.Count)
			{
				damageExtraInfo[j].gameObject.SetActive(value: true);
				damageExtraInfo[j].text.set_text(base.CurrentUnitController.CurrentDamageModifiers[j].GetText(forcePercent: false));
			}
			else
			{
				damageExtraInfo[j].gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateMoreInfoVisibility()
	{
		leftMoreInfoActionGo.gameObject.SetActive(PandoraSingleton<UIMissionManager>.Instance.ShowingMoreInfoUnitAction);
		rightMoreInfoActionGo.gameObject.SetActive(PandoraSingleton<UIMissionManager>.Instance.ShowingMoreInfoUnitAction);
	}

	public void WaitingOpponent()
	{
		currentAction = null;
		OnEnable();
		navigation.SetActive(value: false);
		confirm.gameObject.SetActive(value: false);
		cancel.gameObject.SetActive(value: false);
		((Behaviour)(object)damage).enabled = false;
		((Behaviour)(object)percent).enabled = false;
		noActionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("waiting_opponent"));
		noAction.SetActive(value: true);
		action.set_overrideSprite((Sprite)null);
		((Behaviour)(object)mastery).enabled = false;
		((Behaviour)(object)prevAction).enabled = false;
		((Behaviour)(object)prevMastery).enabled = false;
		((Behaviour)(object)nextAction).enabled = false;
		((Behaviour)(object)nextMastery).enabled = false;
		for (int i = 0; i < strategyPoints.Count; i++)
		{
			strategyPoints[i].SetActive(value: false);
		}
		for (int j = 0; j < offensePoints.Count; j++)
		{
			offensePoints[j].SetActive(value: false);
		}
		for (int k = 0; k < rollExtraInfo.Count; k++)
		{
			rollExtraInfo[k].gameObject.SetActive(value: false);
		}
		for (int l = 0; l < damageExtraInfo.Count; l++)
		{
			damageExtraInfo[l].gameObject.SetActive(value: false);
		}
	}
}
