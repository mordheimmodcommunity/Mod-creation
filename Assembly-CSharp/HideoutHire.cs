using System.Collections;
using System.Collections.Generic;

public class HideoutHire : ICheapState
{
	public List<UnitMenuController> hireUnits;

	private HideoutCamAnchor camAnchor;

	private UnitDescriptionModule unitDescModule;

	private UnitStatsModule statsModule;

	private UnitSheetModule sheetModule;

	private HireUnitSelectionModule unitSelectionModule;

	private CharacterCameraAreaModule characterCamModule;

	private int unitIndex;

	private UnitMenuController currentUnit;

	private Warband warband;

	private bool firstSelectedUnit;

	private bool once = true;

	public HideoutHire(HideoutManager mng, HideoutCamAnchor anchor)
	{
		camAnchor = anchor;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		firstSelectedUnit = true;
		warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
		if (PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx != -1)
		{
			hireUnits = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHireableUnits(PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx, PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotHireImpressive);
		}
		else
		{
			hireUnits = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHiredSwordUnits();
		}
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.UNIT_SHEET, ModuleId.UNIT_STATS);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.UNIT_DESC);
		if (hireUnits.Count > 1)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.HIRE_UNIT_SELECTION, ModuleId.TITLE, ModuleId.CHARACTER_AREA, ModuleId.NEXT_UNIT);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<NextUnitModule>(ModuleId.NEXT_UNIT).Setup();
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.HIRE_UNIT_SELECTION, ModuleId.TITLE, ModuleId.CHARACTER_AREA);
		}
		TitleModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE);
		moduleCenter.Set("hideout_hire");
		statsModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitStatsModule>(ModuleId.UNIT_STATS);
		statsModule.SetInteractable(interactable: false);
		sheetModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
		sheetModule.SetInteractable(interactable: false);
		unitDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<UnitDescriptionModule>(ModuleId.UNIT_DESC);
		characterCamModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CharacterCameraAreaModule>(ModuleId.CHARACTER_AREA);
		characterCamModule.Init(camAnchor.transform.position);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
		unitSelectionModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<HireUnitSelectionModule>(ModuleId.HIRE_UNIT_SELECTION);
		if (PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx != -1)
		{
			unitSelectionModule.Set(hireUnits, Prev, Next, UnitConfirmed, OnDoubleClick);
		}
		else
		{
			unitSelectionModule.Set(hireUnits, Prev, Next, UnitConfirmed, null);
		}
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.NEXT_UNIT, OnNextUnit);
		unitIndex = 0;
		SelectUnit(hireUnits[unitIndex]);
		once = true;
	}

	private void OnNextUnit()
	{
		if ((bool)PandoraSingleton<NoticeManager>.Instance.Parameters[0])
		{
			Prev();
		}
		else
		{
			Next();
		}
	}

	private void UnitConfirmed(int newIndex)
	{
		unitIndex = newIndex;
		if (!once && !PandoraSingleton<HideoutManager>.Instance.showingTuto)
		{
			SelectUnit(hireUnits[newIndex]);
		}
	}

	private void OnDoubleClick(int newIndex)
	{
		UnitConfirmed(newIndex);
		LaunchHirePopup();
	}

	public void Exit(int iTo)
	{
		PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.NEXT_UNIT, OnNextUnit);
		unitSelectionModule.Clear();
		PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
	}

	public void Update()
	{
		if (PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx != -1 && PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
		{
			LaunchHirePopup();
		}
	}

	private void Prev()
	{
		unitIndex = ((unitIndex - 1 < 0) ? (hireUnits.Count - 1) : (unitIndex - 1));
		SelectUnit(hireUnits[unitIndex]);
	}

	private void Next()
	{
		unitIndex = ((unitIndex + 1 < hireUnits.Count) ? (unitIndex + 1) : 0);
		SelectUnit(hireUnits[unitIndex]);
	}

	public void FixedUpdate()
	{
		if (once)
		{
			once = false;
			PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.HIRE);
		}
	}

	public void SelectUnit(UnitMenuController unitCtrlr)
	{
		if (unitCtrlr != null)
		{
			int index = hireUnits.IndexOf(unitCtrlr);
			unitSelectionModule.OnUnitSelected(index);
			PandoraSingleton<HideoutManager>.Instance.currentUnit = unitCtrlr;
			currentUnit = unitCtrlr;
			PandoraSingleton<HideoutManager>.Instance.unitNode.SetContent(currentUnit);
			PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(PandoraSingleton<HideoutManager>.Instance.unitNode.transform, 1.25f);
			currentUnit.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
			statsModule.RefreshStats(unitCtrlr.unit);
			sheetModule.RefreshAttributes(unitCtrlr.unit);
			unitDescModule.Refresh(unitCtrlr.unit, showCost: true);
			if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
			{
				unitDescModule.tabs[0].SetSelected(force: true);
			}
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
			PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnToWarband, mouseOnly: false);
			PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
			characterCamModule.SetCameraLookAtDefault(firstSelectedUnit);
			firstSelectedUnit = false;
		}
	}

	private void LaunchHirePopup()
	{
		if (warband.GetUnitHireCost(currentUnit.unit) > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
		{
			PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_hire"), null, hideButtons: false, hideCancel: true);
		}
		else
		{
			PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentUnit.unit.UnitSave.isOutsider) ? "hideout_hire" : "popup_hire_outsider_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentUnit.unit.UnitSave.isOutsider) ? "popup_hire_unit_desc" : "popup_hire_outsider_desc", currentUnit.unit.LocalizedName, warband.GetUnitHireCost(currentUnit.unit).ToString()), OnHirePopup);
		}
	}

	private void OnHirePopup(bool confirm)
	{
		if (!confirm)
		{
			return;
		}
		PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(warband.GetUnitHireCost(currentUnit.unit));
		PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.HIRED_WARRIORS, 1);
		if (currentUnit.unit.UnitSave.isOutsider)
		{
			PandoraSingleton<HideoutManager>.Instance.currentUnit = currentUnit;
			currentUnit.unit.UnitSave.warbandSlotIndex = PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx;
			if (currentUnit.unit.Rank == 0)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.HireUnit(currentUnit);
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
				ReturnToWarband();
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider = true;
				PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(7);
				PandoraSingleton<GameManager>.Instance.StartCoroutine(ShowOutsiderPopupOnNextFrame());
			}
		}
		else
		{
			PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.HireUnit(currentUnit);
			currentUnit.unit.UnitSave.warbandSlotIndex = PandoraSingleton<HideoutManager>.Instance.currentWarbandSlotIdx;
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			ReturnToWarband();
		}
	}

	private static IEnumerator ShowOutsiderPopupOnNextFrame()
	{
		yield return null;
		yield return null;
		PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("popup_new_outsider_title", "popup_new_outsider_desc", null, hideButtons: false, hideCancel: true);
	}

	private void ReturnToWarband()
	{
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(1);
	}
}
