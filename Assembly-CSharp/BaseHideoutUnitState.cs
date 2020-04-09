using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseHideoutUnitState : ICheapState
{
	protected HideoutCamAnchor camAnchor;

	private HideoutManager.State statemachineState;

	protected UnitTabsModule tabMod;

	protected CharacterCameraAreaModule characterCamModule;

	protected UnitSheetModule sheetModule;

	protected UnitStatsModule statsModule;

	protected DescriptionModule descModule;

	protected UnitMenuController currentUnit;

	private AttributeId attributeSelected;

	private Action applyChangesCallback;

	private bool firstSelectedUnit;

	private Coroutine selectedStatHiding;

	protected BaseHideoutUnitState(HideoutCamAnchor anchor, HideoutManager.State state)
	{
		camAnchor = anchor;
		statemachineState = state;
	}

	protected virtual void OnAttributeSelected(AttributeId attributeId)
	{
		attributeSelected = attributeId;
		switch (attributeId)
		{
		case AttributeId.NONE:
			break;
		case AttributeId.DAMAGE_MIN:
			ShowDescription(PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_weapon_damage"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_desc_weapon_damage"));
			break;
		default:
		{
			string str = attributeId.ToLowerString();
			ShowDescription(PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_name_" + str), PandoraSingleton<LocalizationManager>.Instance.GetStringById("attribute_desc_" + str));
			break;
		}
		}
	}

	protected virtual void OnAttributeUnselected(AttributeId attributeId)
	{
		if (descModule != null)
		{
			if (selectedStatHiding != null)
			{
				PandoraSingleton<HideoutManager>.Instance.StopCoroutine(selectedStatHiding);
			}
			selectedStatHiding = PandoraSingleton<HideoutManager>.Instance.StartCoroutine(HideStatDescDelayed());
		}
	}

	private IEnumerator HideStatDescDelayed()
	{
		yield return new WaitForSeconds(1f);
		HideStatsDesc();
	}

	protected virtual void HideStatsDesc()
	{
		descModule.gameObject.SetActive(value: false);
	}

	protected virtual void ShowDescription(string title, string desc)
	{
		descModule.gameObject.SetActive(value: true);
		descModule.desc.SetLocalized(title, desc);
		if (selectedStatHiding != null)
		{
			PandoraSingleton<HideoutManager>.Instance.StopCoroutine(selectedStatHiding);
			selectedStatHiding = null;
		}
	}

	public void SetupAttributeButtons(ButtonGroup lowerAttributes, ButtonGroup raiseAttributes, ButtonGroup applyChanges)
	{
		if (attributeSelected != 0 && PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.CanLowerAttribute(attributeSelected))
		{
			lowerAttributes.SetAction("raise_attribute", "menu_lower_attribute", 0, negative: true);
			lowerAttributes.OnAction(LowerAttribute, mouseOnly: false);
		}
		else
		{
			lowerAttributes.gameObject.SetActive(value: false);
		}
		if (attributeSelected != 0 && PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.CanRaiseAttribute(attributeSelected))
		{
			raiseAttributes.SetAction("raise_attribute", "menu_raise_attribute");
			raiseAttributes.OnAction(RaiseAttribute, mouseOnly: false);
		}
		else
		{
			raiseAttributes.gameObject.SetActive(value: false);
		}
		if (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.HasPendingChanges())
		{
			if (!applyChanges.isActiveAndEnabled)
			{
				applyChanges.SetAction("action", "menu_apply");
				applyChanges.OnAction(ApplyChanges, mouseOnly: false);
			}
		}
		else
		{
			applyChanges.gameObject.SetActive(value: false);
		}
	}

	private void LowerAttribute()
	{
		PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.LowerAttribute(attributeSelected);
		RefreshUnitAttributes();
		statsModule.SelectStat(attributeSelected);
		OnAttributeSelected(attributeSelected);
	}

	private void RaiseAttribute()
	{
		PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.RaiseAttribute(attributeSelected);
		RefreshUnitAttributes();
		statsModule.SelectStat(attributeSelected);
		OnAttributeSelected(attributeSelected);
	}

	public void SetupApplyButton(ButtonGroup applyChanges)
	{
		if (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.HasPendingChanges())
		{
			applyChanges.SetAction("action", "menu_apply_changes");
			applyChanges.OnAction(ApplyChangesAndResetDescription, mouseOnly: false);
		}
		else
		{
			applyChanges.gameObject.SetActive(value: false);
		}
	}

	private void ApplyChangesAndResetDescription()
	{
		ApplyChanges();
		OnAttributeSelected(attributeSelected);
	}

	private IEnumerator ReloadStateOnNextFrame()
	{
		yield return 0;
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId());
	}

	private void ApplyChanges()
	{
		CheckForChanges(OnApplyChanges);
	}

	public virtual void OnApplyChanges()
	{
		RefreshUnitAttributes();
		OnAttributeSelected(AttributeId.NONE);
	}

	public void ReturnToWarband()
	{
		CheckChangesAndReturnToWarband();
	}

	protected void CheckForChanges(Action callback, bool checkOutsider = false)
	{
		if (checkOutsider && PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
		{
			applyChangesCallback = callback;
			if (currentUnit.unit.UnspentMartial > 0 || currentUnit.unit.UnspentMental > 0 || currentUnit.unit.UnspentPhysical > 0 || currentUnit.unit.UnspentSkill > 0 || currentUnit.unit.UnspentSpell > 0)
			{
				PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("popup_quit_outsider_title", "popup_quit_outsider_desc", OnOutsiderPopup);
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("popup_quit_outsider_no_point_title", "popup_quit_outsider_no_point_desc", OnOutsiderPopup);
			}
		}
		else if (currentUnit != null && currentUnit.unit.HasPendingChanges())
		{
			applyChangesCallback = callback;
			PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("popup_apply_changes_title", "popup_apply_changes_desc", OnApplyPopup);
		}
		else
		{
			callback?.Invoke();
		}
	}

	private void OnOutsiderPopup(bool confirm)
	{
		if (confirm)
		{
			currentUnit.unit.ApplyChanges();
			PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.HireUnit(currentUnit);
			PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider = false;
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			if (applyChangesCallback != null)
			{
				applyChangesCallback();
				applyChangesCallback = null;
			}
		}
	}

	private void OnApplyPopup(bool confirm)
	{
		if (confirm)
		{
			currentUnit.unit.ApplyChanges();
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
		}
		else
		{
			currentUnit.unit.ResetChanges();
		}
		if (applyChangesCallback != null)
		{
			applyChangesCallback();
			applyChangesCallback = null;
		}
	}

	public void CheckChangesAndChangeState(HideoutManager.State newState)
	{
		int stateIndex = (int)newState;
		CheckForChanges(delegate
		{
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(stateIndex);
		});
	}

	public void CheckChangesAndReturnToWarband()
	{
		CheckForChanges(ReturnToWarbandState, checkOutsider: true);
	}

	private void ReturnToWarbandState()
	{
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(1);
	}

	public void ChangeCurrentUnit(UnitMenuController ctrlr, Action callback)
	{
		CheckForChanges(delegate
		{
			currentUnit = ctrlr;
			if (callback != null)
			{
				callback();
			}
		});
	}

	public void Destroy()
	{
	}

	public virtual void Enter(int iFrom)
	{
		Init();
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.UNIT_SHEET, ModuleId.UNIT_STATS);
		statsModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitStatsModule>(ModuleId.UNIT_STATS);
		statsModule.SetInteractable(CanIncreaseAttributes());
		sheetModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
		sheetModule.SetInteractable(CanIncreaseAttributes());
	}

	protected void Init()
	{
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		currentUnit = PandoraSingleton<HideoutManager>.Instance.currentUnit;
		firstSelectedUnit = true;
		tabMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<UnitTabsModule>(ModuleId.UNIT_TABS);
		tabMod.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
		tabMod.SetCurrentTab(statemachineState);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.NEXT_UNIT, OnNextUnit);
	}

	public virtual void Exit(int iTo)
	{
		currentUnit = null;
		if (statsModule != null)
		{
			statsModule.toggleGroup.SetAllTogglesOff();
		}
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.NEXT_UNIT, OnNextUnit);
		PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
		PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	private void OnNextUnit()
	{
		if (!PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
		{
			bool flag = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
			int num = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.IndexOf(currentUnit);
			int num2 = num;
			num = ((!flag) ? ((num + 1 < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count) ? (num + 1) : 0) : ((num - 1 < 0) ? (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count - 1) : (num - 1)));
			if (num != num2)
			{
				ChangeCurrentUnit(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[num], OnUnitChanged);
			}
		}
	}

	private void OnUnitChanged()
	{
		SelectUnit(currentUnit);
	}

	public virtual void SelectUnit(UnitMenuController ctrlr)
	{
		PandoraSingleton<HideoutManager>.Instance.currentUnit = ctrlr;
		currentUnit = ctrlr;
		currentUnit.Hide(hide: false, force: true);
		PandoraSingleton<HideoutManager>.Instance.unitNode.SetContent(currentUnit);
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(PandoraSingleton<HideoutManager>.Instance.unitNode.transform, 1.25f);
		currentUnit.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
		if (statsModule != null)
		{
			if (CanIncreaseAttributes())
			{
				statsModule.RefreshStats(ModuleLeftOnRight(), currentUnit.unit, OnAttributeSelected, OnAttributeChanged, OnAttributeUnselected);
			}
			else
			{
				statsModule.RefreshStats(currentUnit.unit);
			}
		}
		if (sheetModule != null)
		{
			sheetModule.Refresh(ModuleLeftOnRight(), currentUnit.unit, OnAttributeSelected, ShowDescription, OnAttributeUnselected);
		}
		RefreshUnitAttributes();
		tabMod.Refresh();
		if (characterCamModule != null)
		{
			characterCamModule.SetCameraLookAtDefault(firstSelectedUnit);
			firstSelectedUnit = false;
		}
		if (!tabMod.IsTabAvailable(statemachineState))
		{
			PandoraSingleton<HideoutManager>.Instance.StartCoroutine(ReturnToUnitInfo());
		}
	}

	public IEnumerator ReturnToUnitInfo()
	{
		yield return new WaitForEndOfFrame();
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(15);
	}

	public abstract Selectable ModuleLeftOnRight();

	public ToggleEffects ModuleCentertOnLeft()
	{
		return statsModule.stats[0].statSelector;
	}

	public void RefreshUnitAttributes()
	{
		if (sheetModule != null)
		{
			sheetModule.RefreshAttributes(currentUnit.unit);
		}
		if (statsModule != null)
		{
			statsModule.RefreshAttributes(currentUnit.unit);
		}
	}

	protected virtual void OnAttributeChanged()
	{
		RefreshUnitAttributes();
	}

	public abstract bool CanIncreaseAttributes();
}
