using System.Collections.Generic;

public class WarbandManagementBaseState : ICheapState
{
	private List<int> cannotSwapNodes = new List<int>();

	protected HideoutManager hideoutMngr;

	protected WarbandManagementModule warbandManagementModule;

	private ModuleId warbandManagementModuleId;

	protected Unit currentUnit;

	protected int currentUnitSlotIndex;

	protected bool currentUnitIsImpressive;

	protected WarbandManagementBaseState(HideoutManager mng, ModuleId warbandManagementModuleId)
	{
		this.warbandManagementModuleId = warbandManagementModuleId;
		hideoutMngr = mng;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Update()
	{
	}

	public virtual void Enter(int iFrom)
	{
		currentUnit = null;
		currentUnitSlotIndex = -1;
		warbandManagementModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandManagementModule>(warbandManagementModuleId);
		warbandManagementModule.Set(hideoutMngr.WarbandCtrlr.Warband, NodeSelected, NodeConfirmed, ShowSwap, OnHiredSwordsSelected, OnHiredSwordsConfirmed);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
		SetupDefaultButtons();
		warbandManagementModule.leaderSlots[0].slot.SetSelected(force: true);
	}

	public virtual void Exit(int iTo)
	{
	}

	private void NodeSelected(int slotIndex, Unit unit, bool isImpressive)
	{
		currentUnitIsImpressive = isImpressive;
		currentUnitSlotIndex = slotIndex;
		currentUnit = unit;
		if (slotIndex == 0)
		{
			OnIdolSelected();
		}
		else if (currentUnit != null)
		{
			OnUnitSelected(currentUnit);
		}
		else
		{
			OnEmptyNodeSelected(slotIndex, isImpressive);
		}
	}

	private void NodeConfirmed(int slotIndex, Unit unit, bool isImpressive)
	{
		currentUnitIsImpressive = isImpressive;
		currentUnitSlotIndex = slotIndex;
		currentUnit = unit;
		if (slotIndex == 0)
		{
			OnIdolConfirmed();
		}
		else if (currentUnit != null)
		{
			OnUnitConfirmed(currentUnit);
		}
		else
		{
			OnEmptyNodeConfirmed(isImpressive);
		}
	}

	protected virtual void OnEmptyNodeConfirmed(bool isImpressive)
	{
	}

	protected virtual void OnHiredSwordsConfirmed()
	{
	}

	protected virtual void OnHiredSwordsSelected()
	{
	}

	protected virtual void OnIdolSelected()
	{
	}

	protected virtual void OnIdolConfirmed()
	{
	}

	protected virtual void OnUnitSelected(Unit unit)
	{
		if (unit != null)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.UNIT_SHEET, ModuleId.UNIT_QUICK_STATS);
			UnitQuickStatsModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitQuickStatsModule>(ModuleId.UNIT_QUICK_STATS);
			moduleLeft.RefreshStats(unit);
			moduleLeft.SetInteractable(interactable: false);
			UnitSheetModule moduleLeft2 = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
			moduleLeft2.SetInteractable(interactable: false);
			moduleLeft2.Refresh(null, unit);
			SetupDefaultButtons();
		}
	}

	protected virtual void OnUnitConfirmed(Unit unit)
	{
		ShowSwap();
	}

	protected virtual void OnEmptyNodeSelected(int slotIndex, bool isImpressive)
	{
	}

	public virtual void ShowSwap()
	{
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.SWAP);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false);
		WarbandSwapModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSwapModule>(ModuleId.SWAP);
		moduleCenter.Set(hideoutMngr.WarbandCtrlr.Warband, HideSwap, isMission: false, isCampaign: false, isContest: false, null, pushLayer: false);
	}

	protected virtual void HideSwap(bool confirm)
	{
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(warbandManagementModuleId, ModuleId.NOTIFICATION);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
		warbandManagementModule.Set(hideoutMngr.WarbandCtrlr.Warband, NodeSelected, NodeConfirmed, ShowSwap, OnHiredSwordsSelected, OnHiredSwordsConfirmed);
		SetupDefaultButtons();
	}

	protected virtual void SetupDefaultButtons()
	{
	}

	protected void SelectCurrentUnit()
	{
		OnUnitConfirmed(currentUnit);
	}

	public virtual void FixedUpdate()
	{
	}
}
