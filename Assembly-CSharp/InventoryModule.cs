using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryModule : UIModule
{
	public List<ToggleEffects> tabs;

	public UIInventoryItemRuneList itemRuneList;

	public InventoryModuleTab currentTab;

	public GameObject filtersSection;

	public List<ItemTypeTab> itemTypesTabs;

	public Text filterTitle;

	public ButtonGroup btnPreviousFilter;

	public ButtonGroup btnNextFilter;

	private bool isFocused;

	private int selectedFilter;

	private UnityAction<InventoryModuleTab> tabSelected;

	public void Init(UnityAction<InventoryModuleTab> tabSelected)
	{
		base.Init();
		this.tabSelected = tabSelected;
		btnPreviousFilter.SetAction("subfilter", null);
		btnPreviousFilter.OnAction(NextFilter, mouseOnly: false);
		btnPreviousFilter.SetDisabled();
		for (int i = 0; i < tabs.Count; i++)
		{
			tabs[i].onAction.RemoveAllListeners();
			InventoryModuleTab tab = (InventoryModuleTab)i;
			tabs[i].onAction.AddListener(delegate
			{
				SetTab(tab);
			});
		}
		for (int j = 0; j < itemTypesTabs.Count; j++)
		{
			int filterIdx = j;
			itemTypesTabs[j].image.onAction.AddListener(delegate
			{
				SelectFilter(filterIdx);
			});
		}
	}

	public void SelectFilter(int filterIdx)
	{
		filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesTabs[filterIdx].itemType.ToLowerString()));
		itemTypesTabs[filterIdx].image.SetOn();
		selectedFilter = filterIdx;
		tabSelected(currentTab);
	}

	private void NextFilter()
	{
		int num = selectedFilter++;
		if (selectedFilter == itemTypesTabs.Count)
		{
			selectedFilter = 0;
		}
		while (num != selectedFilter && !itemTypesTabs[selectedFilter].image.enabled)
		{
			if (++selectedFilter == itemTypesTabs.Count)
			{
				selectedFilter = 0;
			}
		}
		itemTypesTabs[selectedFilter].image.onAction.Invoke();
		filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesTabs[selectedFilter].itemType.ToLowerString()));
	}

	public void SetTab(InventoryModuleTab idx, bool sendCallback = true)
	{
		tabs[(int)idx].toggle.set_isOn(true);
		currentTab = idx;
		Clear(clearFocus: false);
		if (isFocused && sendCallback)
		{
			tabSelected(idx);
		}
		filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesTabs[selectedFilter].itemType.ToLowerString()));
	}

	public void Clear(bool clearFocus = true)
	{
		if (clearFocus)
		{
			isFocused = false;
			btnPreviousFilter.SetDisabled();
		}
		itemRuneList.Clear();
	}

	private void SetAvailableFilters(UnitSlotId slot)
	{
		switch (slot)
		{
		case UnitSlotId.ARMOR:
		{
			for (int l = 0; l < itemTypesTabs.Count; l++)
			{
				ItemTypeId itemType3 = itemTypesTabs[l].itemType;
				itemTypesTabs[l].image.enabled = (itemType3 == ItemTypeId.NONE || itemType3 == ItemTypeId.CLOTH_ARMOR || itemType3 == ItemTypeId.LIGHT_ARMOR || itemType3 == ItemTypeId.HEAVY_ARMOR);
			}
			break;
		}
		case UnitSlotId.HELMET:
		{
			for (int n = 0; n < itemTypesTabs.Count; n++)
			{
				ItemTypeId itemType5 = itemTypesTabs[n].itemType;
				itemTypesTabs[n].image.enabled = (itemType5 == ItemTypeId.NONE || itemType5 == ItemTypeId.HELMET);
			}
			break;
		}
		case UnitSlotId.SET1_MAINHAND:
		case UnitSlotId.SET2_MAINHAND:
		{
			for (int j = 0; j < itemTypesTabs.Count; j++)
			{
				ItemTypeId itemType = itemTypesTabs[j].itemType;
				itemTypesTabs[j].image.enabled = (itemType == ItemTypeId.NONE || itemType == ItemTypeId.MELEE_1H || itemType == ItemTypeId.MELEE_2H || itemType == ItemTypeId.RANGE || itemType == ItemTypeId.RANGE_FIREARM);
			}
			break;
		}
		case UnitSlotId.SET1_OFFHAND:
		case UnitSlotId.SET2_OFFHAND:
		{
			for (int m = 0; m < itemTypesTabs.Count; m++)
			{
				ItemTypeId itemType4 = itemTypesTabs[m].itemType;
				itemTypesTabs[m].image.enabled = (itemType4 == ItemTypeId.NONE || itemType4 == ItemTypeId.MELEE_1H || itemType4 == ItemTypeId.SHIELD);
			}
			break;
		}
		case UnitSlotId.ITEM_1:
		case UnitSlotId.ITEM_2:
		case UnitSlotId.ITEM_3:
		case UnitSlotId.ITEM_4:
		case UnitSlotId.ITEM_5:
		case UnitSlotId.ITEM_6:
		case UnitSlotId.ITEM_7:
		{
			for (int k = 0; k < itemTypesTabs.Count; k++)
			{
				ItemTypeId itemType2 = itemTypesTabs[k].itemType;
				itemTypesTabs[k].image.enabled = (itemType2 == ItemTypeId.NONE);
			}
			break;
		}
		default:
		{
			for (int i = 0; i < itemTypesTabs.Count; i++)
			{
				itemTypesTabs[i].image.enabled = true;
			}
			break;
		}
		}
		if (!itemTypesTabs[selectedFilter].image.enabled)
		{
			SelectFilter(0);
		}
	}

	public void SetList(List<Item> items, UnityAction<Item> slotConfirmed, UnityAction<Item> slotSelected, UnitSlotId currentSlot, string reason = "", bool slotLocked = false)
	{
		isFocused = true;
		btnPreviousFilter.SetDisabled(disabled: false);
		filtersSection.SetActive(value: true);
		SetAvailableFilters(currentSlot);
		if (itemTypesTabs[selectedFilter].itemType != 0)
		{
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (items[num].TypeData.Id != itemTypesTabs[selectedFilter].itemType)
				{
					items.RemoveAt(num);
				}
			}
		}
		bool addEmpty = currentSlot != UnitSlotId.SET1_MAINHAND && currentSlot != UnitSlotId.ARMOR && currentTab != InventoryModuleTab.SHOP && !slotLocked;
		itemRuneList.SetList(items, slotConfirmed, slotSelected, addEmpty, currentTab == InventoryModuleTab.SHOP, buy: true, flagSold: false, allowHighlight: true, reason);
		itemRuneList.SetFocus();
	}

	public void SetList(List<RuneMark> runeList, List<RuneMark> notAvailableRuneList, UnityAction<RuneMark> slotConfirmed, UnityAction<RuneMark> slotSelected, string reason = null)
	{
		filtersSection.SetActive(value: false);
		isFocused = true;
		btnPreviousFilter.SetDisabled(disabled: false);
		itemRuneList.SetList(runeList, notAvailableRuneList, slotConfirmed, slotSelected, reason);
		itemRuneList.SetFocus();
	}

	private void Update()
	{
		if (isFocused)
		{
			if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
			{
				int idx = (int)(currentTab + 1) % 3;
				SetTab((InventoryModuleTab)idx);
			}
			else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
			{
				int num = (int)(currentTab - 1);
				num = ((num >= 0) ? num : 2);
				SetTab((InventoryModuleTab)num);
			}
		}
	}
}
