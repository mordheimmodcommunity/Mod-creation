using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopModule : UIModule
{
	public List<ToggleEffects> tabs;

	public UIInventoryItemRuneList itemRuneList;

	public ShopModuleTab currentTab;

	public Text emptyMessage;

	public List<ItemTypeTab> itemTypesIcons;

	public Text filterTitle;

	public ButtonGroup btnPreviousFilter;

	public ButtonGroup btnNextFilter;

	private int selectedFilter;

	private UnityAction<ShopModuleTab> tabSelected;

	public void Init(UnityAction<ShopModuleTab> tabSelected)
	{
		base.Init();
		this.tabSelected = tabSelected;
		btnPreviousFilter.SetAction("subfilter", null);
		btnPreviousFilter.OnAction(NextFilter, mouseOnly: false);
		for (int i = 0; i < tabs.Count; i++)
		{
			tabs[i].onAction.RemoveAllListeners();
			ShopModuleTab tab = (ShopModuleTab)i;
			tabs[i].onAction.AddListener(delegate
			{
				SetTab(tab);
			});
		}
		for (int j = 0; j < itemTypesIcons.Count; j++)
		{
			int filterIdx = j;
			itemTypesIcons[j].image.onAction.RemoveAllListeners();
			itemTypesIcons[j].image.onAction.AddListener(delegate
			{
				selectedFilter = filterIdx;
				tabSelected(currentTab);
				filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesIcons[selectedFilter].itemType.ToLowerString()));
			});
		}
	}

	public void SetTab(ShopModuleTab idx)
	{
		tabs[(int)idx].toggle.set_isOn(true);
		currentTab = idx;
		itemRuneList.Clear();
		if (tabSelected != null)
		{
			tabSelected(idx);
		}
		filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesIcons[selectedFilter].itemType.ToLowerString()));
	}

	public void SetList(List<Item> items, UnityAction<Item> slotConfirmed, UnityAction<Item> slotSelected, bool buy)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		ApplyFilter(items);
		Debug.Log("SHOP " + (Time.realtimeSinceStartup - realtimeSinceStartup));
		realtimeSinceStartup = Time.realtimeSinceStartup;
		if (items.Count <= 0)
		{
			switch (currentTab)
			{
			case ShopModuleTab.BUY:
				emptyMessage.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("market_sold_out"));
				break;
			case ShopModuleTab.SELL:
				emptyMessage.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_item_inventory"));
				break;
			default:
				emptyMessage.set_text(string.Empty);
				break;
			}
			((Component)(object)emptyMessage).gameObject.SetActive(value: true);
		}
		else
		{
			((Component)(object)emptyMessage).gameObject.SetActive(value: false);
		}
		Debug.Log("SHOP " + (Time.realtimeSinceStartup - realtimeSinceStartup));
		realtimeSinceStartup = Time.realtimeSinceStartup;
		itemRuneList.SetList(items, slotConfirmed, slotSelected, addEmpty: false, displayPrice: true, buy);
		Debug.Log("SHOP " + (Time.realtimeSinceStartup - realtimeSinceStartup));
	}

	private void PrevFilter()
	{
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.JOYSTICK)
		{
			if (selectedFilter == 0)
			{
				selectedFilter = itemTypesIcons.Count - 1;
			}
			else
			{
				selectedFilter--;
			}
			itemTypesIcons[selectedFilter].image.SetOn();
			itemTypesIcons[selectedFilter].image.onAction.Invoke();
			filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesIcons[selectedFilter].itemType.ToLowerString()));
		}
	}

	private void NextFilter()
	{
		if (selectedFilter == itemTypesIcons.Count - 1)
		{
			selectedFilter = 0;
		}
		else
		{
			selectedFilter++;
		}
		itemTypesIcons[selectedFilter].image.SetOn();
		itemTypesIcons[selectedFilter].image.onAction.Invoke();
		filterTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_item_type_" + itemTypesIcons[selectedFilter].itemType.ToLowerString()));
	}

	private void ApplyFilter(List<Item> items)
	{
		if (itemTypesIcons[selectedFilter].itemType == ItemTypeId.NONE)
		{
			return;
		}
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].TypeData.Id != itemTypesIcons[selectedFilter].itemType)
			{
				switch (itemTypesIcons[selectedFilter].itemType)
				{
				case ItemTypeId.CONSUMABLE_POTIONS:
				case ItemTypeId.RECIPE_ENCHANTMENT_NORMAL:
				case ItemTypeId.RECIPE_ENCHANTMENT_MASTERY:
				case ItemTypeId.CONSUMABLE_MISC:
				case ItemTypeId.CONSUMABLE_OUT_COMBAT:
					if (items[num].TypeData.Id != ItemTypeId.CONSUMABLE_POTIONS && items[num].TypeData.Id != ItemTypeId.CONSUMABLE_OUT_COMBAT && items[num].TypeData.Id != ItemTypeId.CONSUMABLE_MISC && items[num].TypeData.Id != ItemTypeId.RECIPE_ENCHANTMENT_NORMAL && items[num].TypeData.Id != ItemTypeId.RECIPE_ENCHANTMENT_MASTERY)
					{
						items.RemoveAt(num);
					}
					break;
				case ItemTypeId.MELEE_1H:
					if (items[num].TypeData.Id != ItemTypeId.SHIELD)
					{
						items.RemoveAt(num);
					}
					break;
				default:
					items.RemoveAt(num);
					break;
				}
			}
		}
	}

	private void Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
		{
			int tab = (int)(currentTab + 1) % 2;
			SetTab((ShopModuleTab)tab);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
		{
			int num = (int)(currentTab - 1);
			num = ((num < 0) ? 1 : num);
			SetTab((ShopModuleTab)num);
		}
	}

	public void RemoveItem(Item item)
	{
		itemRuneList.RemoveItem(item);
	}
}
