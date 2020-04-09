using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarbandSwapModule : WarbandSlotPlacementModule
{
	public ButtonGroup btnLaunch;

	public ButtonGroup btnLaunchDeploy;

	public Image warbandIcon;

	public Text warbandName;

	public Text message;

	public GameObject unitSheetPrefab;

	public GameObject unitQuickStatsPrefab;

	public Unit firstUnit;

	public Transform firstUnitAnchor;

	private UnitSheetModule firstUnitSheet;

	private UnitQuickStatsModule firstUnitStats;

	public Unit secondUnit;

	public Transform secondUnitAnchor;

	private UnitSheetModule secondUnitSheet;

	private UnitQuickStatsModule secondUnitStats;

	protected int firstUnitSlotIndex;

	protected int secondUnitSlotIndex;

	private Action<bool> onClose;

	private bool layerPushed;

	private bool missionMode;

	private List<int> cannotSwapNodes = new List<int>();

	private int maxSlotCount;

	public bool DeployRequested
	{
		get;
		private set;
	}

	public bool HasChanged
	{
		get;
		set;
	}

	public override void Init()
	{
		base.Init();
		GameObject gameObject = UnityEngine.Object.Instantiate(unitSheetPrefab);
		gameObject.transform.SetParent(firstUnitAnchor, worldPositionStays: false);
		firstUnitSheet = gameObject.GetComponent<UnitSheetModule>();
		gameObject = UnityEngine.Object.Instantiate(unitQuickStatsPrefab);
		gameObject.transform.SetParent(firstUnitAnchor, worldPositionStays: false);
		firstUnitStats = gameObject.GetComponent<UnitQuickStatsModule>();
		gameObject = UnityEngine.Object.Instantiate(unitSheetPrefab);
		gameObject.transform.SetParent(secondUnitAnchor, worldPositionStays: false);
		secondUnitSheet = gameObject.GetComponent<UnitSheetModule>();
		gameObject = UnityEngine.Object.Instantiate(unitQuickStatsPrefab);
		gameObject.transform.SetParent(secondUnitAnchor, worldPositionStays: false);
		secondUnitStats = gameObject.GetComponent<UnitQuickStatsModule>();
		firstUnitSheet.gameObject.SetActive(value: false);
		firstUnitStats.gameObject.SetActive(value: false);
		secondUnitSheet.gameObject.SetActive(value: false);
		secondUnitStats.gameObject.SetActive(value: false);
	}

	public void Set(Warband warband, Action<bool> close, bool isMission, bool isCampaign, bool isContest, List<int> unitPosition, bool pushLayer, int ratingMin = 0, int ratingMax = 9999)
	{
		Set(warband, unitPosition, ratingMin, ratingMax);
		onClose = close;
		HasChanged = false;
		missionMode = isMission;
		warbandIcon.set_sprite(Warband.GetIcon(currentWarband.Id));
		warbandName.set_text(currentWarband.GetWarbandSave().Name);
		SetupAvailableSlots();
		firstUnit = null;
		firstUnitSheet.gameObject.SetActive(value: false);
		firstUnitStats.gameObject.SetActive(value: false);
		secondUnit = null;
		secondUnitSheet.gameObject.SetActive(value: false);
		secondUnitStats.gameObject.SetActive(value: false);
		maxSlotCount = 12 + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSlots().Reserve;
		leaderSlots[0].SetSelected(force: true);
		DeployRequested = false;
		layerPushed = pushLayer;
		if (pushLayer)
		{
			PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.POP_UP);
		}
		PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_back", PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
		{
			Close(valid: false);
		}, mouseOnly: false);
		btnLaunch.SetAction(null, "menu_launch_mission", PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer);
		btnLaunch.OnAction(delegate
		{
			Close(valid: true);
		}, mouseOnly: false);
		btnLaunch.gameObject.SetActive(missionMode);
		btnLaunchDeploy.SetAction(null, "menu_launch_mission_deploy", PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer);
		btnLaunchDeploy.OnAction(delegate
		{
			DeployRequested = true;
			Close(valid: true);
		}, mouseOnly: false);
		btnLaunchDeploy.gameObject.SetActive(missionMode && !isCampaign);
		if (isMission || isContest)
		{
			CheckCanLaunchMission();
		}
		else
		{
			((Behaviour)(object)message).enabled = false;
		}
	}

	public void ForceClose()
	{
		if (layerPushed)
		{
			PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.POP_UP);
		}
	}

	private void Close(bool valid)
	{
		ForceClose();
		if (onClose != null)
		{
			onClose(valid);
		}
	}

	public bool CanLaunchMission()
	{
		string reason;
		return CanLaunchMission(out reason);
	}

	public bool CanLaunchMission(out string reason)
	{
		Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(2);
		if (unitAtWarbandSlot == null || (unitsPosition == null && unitAtWarbandSlot.GetActiveStatus() != 0))
		{
			reason = PandoraSingleton<LocalizationManager>.Instance.GetStringById((unitsPosition != null) ? "na_hideout_leader" : "na_hideout_active_leader");
			return false;
		}
		if (GetActiveUnitsCount() < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
		{
			reason = PandoraSingleton<LocalizationManager>.Instance.GetStringById((unitsPosition != null) ? "na_hideout_min_unit" : "na_hideout_min_active_unit");
			return false;
		}
		if (!PandoraUtils.IsBetween(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetSkirmishRating(unitsPosition), warbandRatingMin, warbandRatingMax))
		{
			reason = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rating_invalid", warbandRatingMin.ToConstantString(), warbandRatingMax.ToConstantString());
			return false;
		}
		bool flag = false;
		for (int i = 2; i < 12; i++)
		{
			Unit unitAtWarbandSlot2 = GetUnitAtWarbandSlot(i);
			if (unitAtWarbandSlot2 != null && unitAtWarbandSlot2.GetActiveStatus() != 0)
			{
				flag = true;
			}
		}
		reason = ((!flag) ? string.Empty : PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_hideout_warning_unavailable"));
		return true;
	}

	private void CheckCanLaunchMission()
	{
		if (!CanLaunchMission(out string reason))
		{
			((Behaviour)(object)message).enabled = true;
			message.set_text(reason);
			if (missionMode)
			{
				btnLaunch.SetDisabled();
				if (btnLaunchDeploy.gameObject.activeInHierarchy)
				{
					btnLaunchDeploy.SetDisabled();
				}
			}
			return;
		}
		((Behaviour)(object)message).enabled = true;
		message.set_text((!string.IsNullOrEmpty(reason)) ? reason : string.Empty);
		if (missionMode)
		{
			btnLaunch.SetDisabled(disabled: false);
			if (btnLaunchDeploy.gameObject.activeInHierarchy)
			{
				btnLaunchDeploy.SetDisabled(disabled: false);
			}
		}
	}

	protected override void OnUnitSlotOver(int slotIndex, Unit unit, bool isImpressive)
	{
	}

	protected override void OnUnitSlotSelected(int slotIndex, Unit unit, bool isImpressive)
	{
		if (unit != null && firstUnit == null)
		{
			firstUnitSheet.gameObject.SetActive(value: true);
			firstUnitSheet.SetInteractable(interactable: false);
			firstUnitSheet.Refresh(null, unit);
			firstUnitStats.gameObject.SetActive(value: true);
			firstUnitStats.SetInteractable(interactable: false);
			firstUnitStats.RefreshStats(unit);
			secondUnitSheet.gameObject.SetActive(value: false);
			secondUnitStats.gameObject.SetActive(value: false);
		}
		else if (unit != firstUnit && IsSlotAvailableForSwap(slotIndex))
		{
			if (unit != null)
			{
				secondUnitSheet.gameObject.SetActive(value: true);
				secondUnitSheet.SetInteractable(interactable: false);
				secondUnitSheet.Refresh(null, unit);
				secondUnitStats.gameObject.SetActive(value: true);
				secondUnitStats.SetInteractable(interactable: false);
				secondUnitStats.RefreshStats(unit, firstUnit);
			}
			else
			{
				secondUnitSheet.gameObject.SetActive(value: false);
				secondUnitStats.gameObject.SetActive(value: false);
			}
			firstUnitStats.RefreshStats(firstUnit, unit);
		}
	}

	protected override void OnUnitSlotConfirmed(int slotIndex, Unit currentUnit, bool isImpressive)
	{
		if (currentUnit == null && isImpressive)
		{
			currentUnit = GetUnitAtWarbandSlot(slotIndex);
		}
		if (currentUnit != null || isImpressive)
		{
			if (firstUnit != null && currentUnit != firstUnit && IsSlotAvailableForSwap(slotIndex))
			{
				secondUnit = currentUnit;
				secondUnitSlotIndex = slotIndex;
				SwapUnits();
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
				HasChanged = true;
				FinishSwap();
				currentUnit = firstUnit;
				firstUnit = null;
				OnUnitSlotSelected(slotIndex, currentUnit, isImpressive: false);
			}
			else if (firstUnit == currentUnit)
			{
				((Graphic)allSlots[slotIndex].icon).set_color(Color.white);
				firstUnit = null;
				FinishSwap();
			}
			else
			{
				firstUnit = currentUnit;
				firstUnitSlotIndex = slotIndex;
				FindSwappingNodes(firstUnit);
				StartSwap(firstUnitSlotIndex, cannotSwapNodes, isImpressive);
			}
		}
		else if (firstUnit != null && IsSlotAvailableForSwap(slotIndex))
		{
			SetUnitSlotIndex(firstUnit, slotIndex);
			firstUnit = null;
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			HasChanged = true;
			FinishSwap();
			OnUnitSlotSelected(slotIndex, currentUnit, isImpressive: false);
		}
	}

	private bool IsSlotAvailableForSwap(int slotIndex)
	{
		for (int i = 0; i < cannotSwapNodes.Count; i++)
		{
			if (cannotSwapNodes[i] == slotIndex)
			{
				return false;
			}
		}
		return true;
	}

	private bool CanSwapImpressive(Unit srcImpressiveUnit, int srcSlotIndex, Unit dstUnit, int dstSlotIndex, Unit dstUnit2)
	{
		UnitId unitId = dstUnit?.Id ?? UnitId.NONE;
		UnitId unitId2 = dstUnit2?.Id ?? UnitId.NONE;
		if (currentWarband.IsActiveWarbandSlot(srcSlotIndex) && unitId == unitId2 && unitId != 0 && GetActiveUnitIdCount(unitId) + 2 > dstUnit.Data.MaxCount)
		{
			return false;
		}
		if ((dstUnit != null && !CanPlaceUnitAt(dstUnit, srcSlotIndex)) || (dstUnit2 != null && !CanPlaceUnitAt(dstUnit2, srcSlotIndex + 1)) || (srcImpressiveUnit != null && !CanPlaceUnitAt(srcImpressiveUnit, dstSlotIndex)))
		{
			return false;
		}
		return true;
	}

	private bool FindSwappingNodes(Unit srcUnit)
	{
		cannotSwapNodes.Clear();
		bool result = false;
		int unitSlotIndex = GetUnitSlotIndex(srcUnit);
		bool isImpressive = srcUnit.IsImpressive;
		for (int i = 2; i < maxSlotCount; i++)
		{
			if (isImpressive && unitSlotIndex == i + 1)
			{
				cannotSwapNodes.Add(i + 1);
				continue;
			}
			Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(i);
			bool flag = unitAtWarbandSlot?.IsImpressive ?? false;
			if (isImpressive && flag)
			{
				result = true;
			}
			else if (isImpressive)
			{
				if (CanSwapImpressive(srcUnit, unitSlotIndex, unitAtWarbandSlot, i, GetUnitAtWarbandSlot(i + 1)))
				{
					result = true;
				}
				else
				{
					cannotSwapNodes.Add(i);
				}
				i++;
			}
			else if (flag)
			{
				int linkedImpressiveSlotIndex = GetLinkedImpressiveSlotIndex(unitSlotIndex);
				if (linkedImpressiveSlotIndex + 1 < maxSlotCount)
				{
					Unit linkedImpressiveSlotUnit = GetLinkedImpressiveSlotUnit(unitSlotIndex);
					if (CanSwapImpressive(unitAtWarbandSlot, i, srcUnit, linkedImpressiveSlotIndex, linkedImpressiveSlotUnit))
					{
						result = true;
					}
					else
					{
						cannotSwapNodes.Add(i);
					}
				}
				else
				{
					cannotSwapNodes.Add(i);
				}
				i++;
			}
			else if (!CanPlaceUnitAt(srcUnit, i))
			{
				cannotSwapNodes.Add(i);
			}
			else if (unitAtWarbandSlot == null)
			{
				result = true;
			}
			else if (!CanPlaceUnitAt(unitAtWarbandSlot, unitSlotIndex))
			{
				cannotSwapNodes.Add(i);
			}
		}
		return result;
	}

	private int GetLinkedImpressiveSlotIndex(int slotIndex)
	{
		if (slotIndex >= 20)
		{
			return slotIndex;
		}
		if (slotIndex >= 12)
		{
			if ((slotIndex - 12) % 2 == 0)
			{
				return slotIndex;
			}
			return slotIndex - 1;
		}
		if ((slotIndex - 5) % 2 == 0)
		{
			return slotIndex;
		}
		return slotIndex - 1;
	}

	private Unit GetLinkedImpressiveSlotUnit(int slotIndex)
	{
		if (slotIndex >= 20)
		{
			return null;
		}
		if (slotIndex >= 12)
		{
			if ((slotIndex - 12) % 2 == 0)
			{
				return GetUnitAtWarbandSlot(slotIndex + 1);
			}
		}
		else if ((slotIndex - 5) % 2 == 0)
		{
			return GetUnitAtWarbandSlot(slotIndex + 1);
		}
		return GetUnitAtWarbandSlot(slotIndex - 1);
	}

	public void StartSwap(int fromSlotIndex, List<int> cannotSwapIndex, bool isImpressive)
	{
		Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(fromSlotIndex);
		for (int i = 0; i < allSlots.Count; i++)
		{
			UIUnitSlot uIUnitSlot = allSlots[i];
			if (uIUnitSlot == null || uIUnitSlot.isLocked)
			{
				continue;
			}
			if (cannotSwapIndex.Contains(i) || (isImpressive && i < 20) || (fromSlotIndex >= 20 && unitAtWarbandSlot != null && unitAtWarbandSlot.IsImpressive))
			{
				if (uIUnitSlot.currentUnitAtSlot == null)
				{
					((Graphic)uIUnitSlot.icon).set_color(Color.white);
					uIUnitSlot.icon.set_overrideSprite(noneIcon);
				}
				uIUnitSlot.Deactivate();
			}
			else if (uIUnitSlot.currentUnitAtSlot == null)
			{
				((Graphic)uIUnitSlot.icon).set_color(Color.white);
				uIUnitSlot.icon.set_overrideSprite(swapIcon);
			}
		}
		if (isImpressive)
		{
			for (int j = 0; j < allImpressiveSlots.Count; j++)
			{
				UIUnitSlot uIUnitSlot2 = allImpressiveSlots[j];
				if (uIUnitSlot2.isLocked)
				{
					continue;
				}
				if (uIUnitSlot2.slotTypeIndex == fromSlotIndex)
				{
					((Graphic)uIUnitSlot2.icon).set_color(Constant.GetColor(ConstantId.COLOR_CYAN));
					continue;
				}
				if (cannotSwapIndex.Contains(uIUnitSlot2.slotTypeIndex))
				{
					allSlots[uIUnitSlot2.slotTypeIndex].Deactivate();
					allSlots[uIUnitSlot2.slotTypeIndex + 1].Deactivate();
					uIUnitSlot2.Deactivate();
					continue;
				}
				uIUnitSlot2.Activate();
				if (uIUnitSlot2.currentUnitAtSlot == null)
				{
					((Graphic)uIUnitSlot2.icon).set_color(Color.white);
					uIUnitSlot2.icon.set_overrideSprite(swapIcon);
				}
			}
			return;
		}
		((Graphic)allSlots[fromSlotIndex].icon).set_color(Constant.GetColor(ConstantId.COLOR_CYAN));
		if (fromSlotIndex >= 20 && unitAtWarbandSlot != null && unitAtWarbandSlot.IsImpressive)
		{
			for (int k = 0; k < allImpressiveSlots.Count; k++)
			{
				UIUnitSlot uIUnitSlot3 = allImpressiveSlots[k];
				if (!uIUnitSlot3.isLocked)
				{
					uIUnitSlot3.Activate();
					if (uIUnitSlot3.currentUnitAtSlot == null)
					{
						((Graphic)uIUnitSlot3.icon).set_color(Color.white);
						uIUnitSlot3.icon.set_overrideSprite(swapIcon);
					}
				}
			}
			return;
		}
		for (int l = 0; l < allImpressiveSlots.Count; l++)
		{
			UIUnitSlot uIUnitSlot4 = allImpressiveSlots[l];
			if (!uIUnitSlot4.isLocked)
			{
				if (cannotSwapIndex.Contains(uIUnitSlot4.slotTypeIndex) || cannotSwapIndex.Contains(uIUnitSlot4.slotTypeIndex + 1) || uIUnitSlot4.currentUnitAtSlot == null)
				{
					uIUnitSlot4.Deactivate();
				}
				else
				{
					uIUnitSlot4.Activate();
				}
			}
		}
	}

	private void SwapUnits()
	{
		if (secondUnit != null && secondUnit.IsImpressive)
		{
			int linkedImpressiveSlotIndex = GetLinkedImpressiveSlotIndex(firstUnitSlotIndex);
			Unit linkedImpressiveSlotUnit = GetLinkedImpressiveSlotUnit(firstUnitSlotIndex);
			SetUnitSlotIndex(secondUnit, linkedImpressiveSlotIndex);
			if (linkedImpressiveSlotIndex - firstUnitSlotIndex == 0)
			{
				SetUnitSlotIndex(firstUnit, secondUnitSlotIndex);
				if (linkedImpressiveSlotUnit != null)
				{
					SetUnitSlotIndex(linkedImpressiveSlotUnit, secondUnitSlotIndex + 1, checkIfAvailable: true);
				}
			}
			else
			{
				SetUnitSlotIndex(firstUnit, secondUnitSlotIndex + 1, checkIfAvailable: true);
				if (linkedImpressiveSlotUnit != null)
				{
					SetUnitSlotIndex(linkedImpressiveSlotUnit, secondUnitSlotIndex);
				}
			}
			return;
		}
		if (secondUnit == null)
		{
			SetUnitSlotIndex(firstUnit, secondUnitSlotIndex);
		}
		else
		{
			SetUnitSlotIndex(firstUnit, secondUnitSlotIndex);
			SetUnitSlotIndex(secondUnit, firstUnitSlotIndex);
		}
		if (firstUnit.IsImpressive && (secondUnit == null || !secondUnit.IsImpressive))
		{
			Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(secondUnitSlotIndex + 1);
			if (unitAtWarbandSlot != null)
			{
				SetUnitSlotIndex(unitAtWarbandSlot, firstUnitSlotIndex + 1, checkIfAvailable: true);
			}
		}
	}

	public void FinishSwap()
	{
		SetupAvailableSlots();
		firstUnitSheet.gameObject.SetActive(value: false);
		firstUnitStats.gameObject.SetActive(value: false);
		secondUnitSheet.gameObject.SetActive(value: false);
		secondUnitStats.gameObject.SetActive(value: false);
		CheckCanLaunchMission();
	}
}
