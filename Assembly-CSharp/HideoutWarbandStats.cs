using UnityEngine;
using UnityEngine.UI;

public class HideoutWarbandStats : ICheapState
{
	private HideoutCamAnchor camAnchor;

	private Warband warband;

	private WarbandSheetModule warbandSheet;

	private WarbandOverviewUnitsModule unitOverviewModule;

	private TreasuryModule treasuryModule;

	private StatusDescModule statusDescModule;

	private WarbandOverviewModule warbandOverviewModule;

	public HideoutWarbandStats(HideoutManager mng, HideoutCamAnchor anchor)
	{
		camAnchor = anchor;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION, ModuleId.UNIT_DESC);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.WARBAND_OVERVIEW_UNITS, ModuleId.WARBAND_NEXT_RANK_PREVIEW);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.WARBAND_OVERVIEW);
		warbandOverviewModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<WarbandOverviewModule>(ModuleId.WARBAND_OVERVIEW);
		warbandOverviewModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
		warbandOverviewModule.ShowPanel(0);
		unitOverviewModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandOverviewUnitsModule>(ModuleId.WARBAND_OVERVIEW_UNITS);
		warbandSheet = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandSheetModule>(ModuleId.WARBAND_SHEET);
		treasuryModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
		statusDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<StatusDescModule>(ModuleId.UNIT_DESC);
		statusDescModule.onPayUpkeep = PayAllUpkeep;
		statusDescModule.onPayTreatment = PayAllTreatment;
		statusDescModule.onFireUnit = DisbandWarband;
		statusDescModule.Refresh(warband);
		statusDescModule.SetNav(null, (Selectable)(object)warbandOverviewModule.tabs[0].toggle);
		statusDescModule.SetFocus();
		Cloth cloth = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner.GetComponentsInChildren<Cloth>(includeInactive: true)[0];
		cloth.enabled = false;
		PandoraSingleton<HideoutManager>.Instance.warbandNodeFlag.SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner);
		cloth.enabled = true;
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnToWarband, mouseOnly: false);
		if (PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.UGC))
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("rename_warband", "hideout_rename_warband");
			PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(RenameWarband, mouseOnly: false);
		}
		PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
	}

	public void Exit(int iTo)
	{
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}

	private void ReturnToWarband()
	{
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(1);
	}

	private void RenameWarband()
	{
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
		{
			if (!PandoraSingleton<Hephaestus>.Instance.ShowVirtualKeyboard(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rename_warband"), warband.GetWarbandSave().Name, (uint)Constant.GetInt(ConstantId.MAX_WARBAND_NAME_LENGTH), multiLine: false, OnRenameWarbandDialog))
			{
				PandoraSingleton<HideoutManager>.Instance.textInputPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rename_warband"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rename_warband_desc"), OnRenameWarbandDialog, hideButtons: false, warband.GetWarbandSave().Name, Constant.GetInt(ConstantId.MAX_WARBAND_NAME_LENGTH));
			}
		}
		else
		{
			PandoraSingleton<HideoutManager>.Instance.textInputPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rename_warband"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rename_warband_desc"), OnRenameWarbandDialog, hideButtons: false, warband.GetWarbandSave().Name, Constant.GetInt(ConstantId.MAX_WARBAND_NAME_LENGTH));
		}
	}

	private void OnRenameWarbandDialog(bool confirm, string newName)
	{
		if (confirm && !string.IsNullOrEmpty(newName))
		{
			warband.GetWarbandSave().overrideName = newName;
			warbandSheet.Set(warband);
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.RENAME);
		}
	}

	private void DisbandWarband()
	{
		PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_disband"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_disband_desc"), OnDisbandWarbandDialog);
		PandoraSingleton<HideoutManager>.Instance.messagePopup.cancelButton.effects.toggle.set_isOn(true);
	}

	private void OnDisbandWarbandDialog(bool confirm)
	{
		if (confirm)
		{
			int num = warband.GetTotalUpkeepOwned() + warband.GetTotalTreatmentOwned();
			if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= num)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(num);
				treasuryModule.Refresh(warband.GetWarbandSave());
				warband.PayAllUpkeepOwned();
				warband.PayAllTreatmentOwned();
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.DisbandWarband();
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
				unitOverviewModule.Set(warband);
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_upkeep"), null);
				PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
			}
		}
	}

	private void PayAllUpkeep()
	{
		int totalUpkeepOwned = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetTotalUpkeepOwned();
		PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_btn_pay_all_upkeep"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_pay_all_upkeep_desc", totalUpkeepOwned.ToString(), PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold().ToString()), OnPayUpkeepDialog);
	}

	private void OnPayUpkeepDialog(bool confirm)
	{
		if (confirm)
		{
			int totalUpkeepOwned = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetTotalUpkeepOwned();
			if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= totalUpkeepOwned)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(totalUpkeepOwned);
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.PayAllUpkeepOwned();
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
				treasuryModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
				unitOverviewModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				warbandSheet.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				statusDescModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				statusDescModule.SetNav(null, (Selectable)(object)warbandOverviewModule.tabs[0].toggle);
				statusDescModule.SetFocus();
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_upkeep"), null);
				PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
			}
		}
	}

	private void PayAllTreatment()
	{
		int totalTreatmentOwned = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetTotalTreatmentOwned();
		PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_pay_all_treatment"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_pay_all_treatment_desc", totalTreatmentOwned.ToString(), PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold().ToString()), OnPayTreatmentDialog);
	}

	private void OnPayTreatmentDialog(bool confirm)
	{
		if (confirm)
		{
			int totalTreatmentOwned = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetTotalTreatmentOwned();
			if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= totalTreatmentOwned)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(totalTreatmentOwned);
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.PayAllTreatmentOwned();
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
				treasuryModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
				unitOverviewModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				warbandSheet.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				statusDescModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
				statusDescModule.SetNav(null, (Selectable)(object)warbandOverviewModule.tabs[0].toggle);
				statusDescModule.SetFocus();
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_upkeep"), null);
				PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
			}
		}
	}
}
