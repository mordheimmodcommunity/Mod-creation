using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WheelModule : UIModule
{
	public List<UIWheelSlot> itemSlots;

	public List<UIWheelMutationSlot> mutationSlots;

	private UnityAction<UnitSlotId> onItemSlotConfirmed;

	private UnityAction<int> onMutationSlotConfirmed;

	private UnityAction<UnitSlotId> onShowSlotDescription;

	private UnityAction<int> onShowMutationDescription;

	public ToggleGroup toggleGroup;

	private bool isLocked;

	public GameObject mainWeaponLinkGroup;

	public GameObject secondaryWeaponLinkGroup;

	public Sprite freeSlotIcon;

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < itemSlots.Count; i++)
		{
			int idx = i;
			itemSlots[i].slot.onAction.AddListener(delegate
			{
				ConfirmItemSlot(idx);
			});
			itemSlots[i].slot.onSelect.AddListener(delegate
			{
				ShowItemSlotDescription(idx);
			});
			itemSlots[i].slot.onPointerEnter.AddListener(delegate
			{
				ShowItemSlotDescription(idx);
			});
		}
		for (int j = 0; j < mutationSlots.Count; j++)
		{
			int idx2 = j;
			mutationSlots[j].slot.onAction.AddListener(delegate
			{
				ConfirmMutationSlot(idx2);
			});
			mutationSlots[j].slot.onSelect.AddListener(delegate
			{
				ShowMutationSlotDescription(idx2);
			});
			mutationSlots[j].slot.onPointerEnter.AddListener(delegate
			{
				ShowMutationSlotDescription(idx2);
			});
		}
	}

	public void Activate(ToggleEffects left, UnityAction<UnitSlotId> showSlotDescription, UnityAction<int> showMutationDescription, UnityAction<UnitSlotId> slotConfirmed, UnityAction<int> mutationConfirmed)
	{
		SetInteractable(interactable: true);
		onItemSlotConfirmed = slotConfirmed;
		onShowSlotDescription = showSlotDescription;
		onMutationSlotConfirmed = mutationConfirmed;
		onShowMutationDescription = showMutationDescription;
		for (int i = 0; i <= 5; i++)
		{
			if (i != 3 && i != 5)
			{
				itemSlots[i].SetLeftSelectable(left);
			}
		}
		for (int j = 0; j < mutationSlots.Count; j++)
		{
			mutationSlots[j].SetLeftSelectable(left);
		}
	}

	public void Deactivate()
	{
		SetInteractable(interactable: false);
		onItemSlotConfirmed = null;
	}

	public void ConfirmItemSlot(int idx)
	{
		if (onItemSlotConfirmed != null)
		{
			onItemSlotConfirmed((UnitSlotId)idx);
		}
	}

	public void ConfirmMutationSlot(int idx)
	{
		if (onMutationSlotConfirmed != null)
		{
			onMutationSlotConfirmed(mutationSlots[idx].unitMutationIdx);
		}
	}

	public void ShowItemSlotDescription(int idx)
	{
		if (onShowSlotDescription != null && !isLocked)
		{
			onShowSlotDescription((UnitSlotId)idx);
		}
	}

	public void ShowMutationSlotDescription(int idx)
	{
		if (onShowMutationDescription != null && !isLocked)
		{
			onShowMutationDescription(mutationSlots[idx].unitMutationIdx);
		}
	}

	public void SelectSlot(int idx)
	{
		itemSlots[idx].slot.SetOn();
		itemSlots[idx].slot.SetSelected(force: true);
	}

	public void RefreshSlots(UnitMenuController unitController)
	{
		Unit unit = unitController.unit;
		secondaryWeaponLinkGroup.SetActive(unitController.CanSwitchWeapon());
		for (int i = 0; i < itemSlots.Count; i++)
		{
			bool flag = i >= unit.Items.Count;
			if (!flag)
			{
				flag = ((i == 3 || i == 5) && (unit.Items[i - 1].IsPaired || unit.Items[i - 1].IsTwoHanded));
			}
			if (!flag)
			{
				flag = (unit.GetInjury((UnitSlotId)i) != InjuryId.NONE);
			}
			if (!flag)
			{
				flag = ((i == 4 || i == 5) && unit.GetMutationId((UnitSlotId)i) != MutationId.NONE);
			}
			if (!flag)
			{
				flag = ((i == 4 || i == 5) && unit.Items[i - 2].IsLockSlot);
			}
			if (!flag)
			{
				flag = ((i == 3 || i == 5) && unit.Items[i - 1].Id == ItemId.NONE);
			}
			if (!flag)
			{
				flag = (i >= 7 && (unit.BothArmsMutated() || unit.Data.UnitSizeId == UnitSizeId.LARGE || unit.IsUnitActionBlocked(UnitActionId.CONSUMABLE)));
			}
			itemSlots[i].slot.gameObject.SetActive(value: true);
			if (flag)
			{
				((Selectable)itemSlots[i].slot.toggle).set_interactable(false);
				itemSlots[i].icon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item/none", cached: true));
				((Graphic)itemSlots[i].icon).set_color(Color.white);
				if ((Object)(object)itemSlots[i].subIcon != null)
				{
					((Component)(object)itemSlots[i].subIcon).gameObject.SetActive(value: false);
				}
				continue;
			}
			((Selectable)itemSlots[i].slot.toggle).set_interactable(true);
			if (unit.Items[i].Id == ItemId.NONE)
			{
				itemSlots[i].icon.set_sprite(freeSlotIcon);
				((Graphic)itemSlots[i].icon).set_color(Color.white);
			}
			else
			{
				itemSlots[i].icon.set_sprite(unit.Items[i].GetIcon());
				((Graphic)itemSlots[i].icon).set_color(PandoraUtils.StringToColor(unit.Items[i].QualityData.Color));
			}
			Sprite runeIcon = unit.Items[i].GetRuneIcon();
			if (runeIcon != null)
			{
				((Component)(object)itemSlots[i].subIcon).gameObject.SetActive(value: true);
				itemSlots[i].subIcon.set_sprite(runeIcon);
				((Graphic)itemSlots[i].icon).set_color(PandoraUtils.StringToColor(unit.Items[i].QualityData.Color));
			}
			else if ((Object)(object)itemSlots[i].subIcon != null)
			{
				((Component)(object)itemSlots[i].subIcon).gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < mutationSlots.Count; j++)
		{
			bool flag2 = false;
			for (int k = 0; k < unit.Mutations.Count; k++)
			{
				if (unit.Mutations[k].HasBodyPart(mutationSlots[j].partId))
				{
					flag2 = true;
					mutationSlots[j].slot.gameObject.SetActive(value: true);
					((Selectable)mutationSlots[j].slot.toggle).set_interactable(true);
					mutationSlots[j].icon.set_sprite(unit.Mutations[k].GetIcon());
					((Graphic)mutationSlots[j].icon).set_color(Color.white);
					mutationSlots[j].unitMutationIdx = k;
					if ((Object)(object)mutationSlots[j].subIcon != null)
					{
						((Component)(object)mutationSlots[j].subIcon).gameObject.SetActive(value: false);
					}
					break;
				}
			}
			if (!flag2)
			{
				if (!mutationSlots[j].hiddingSlot)
				{
					mutationSlots[j].slot.gameObject.SetActive(value: false);
				}
				mutationSlots[j].unitMutationIdx = -1;
			}
		}
		for (int l = 0; l < itemSlots.Count; l++)
		{
			itemSlots[l].RefreshNavigation();
		}
		for (int m = 0; m < mutationSlots.Count; m++)
		{
			mutationSlots[m].RefreshNavigation();
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)toggleGroup != null)
		{
			toggleGroup.SetAllTogglesOff();
		}
	}

	public void Lock()
	{
		isLocked = true;
	}

	public void Unlock()
	{
		isLocked = false;
	}
}
