using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIInventoryItemRuneList : MonoBehaviour
{
	public GameObject itemPrefab;

	private List<Item> items = new List<Item>();

	private UnityAction<Item> itemSlotConfirmed;

	private UnityAction<Item> itemSlotSelected;

	public GameObject runePrefab;

	private List<RuneMark> runes = new List<RuneMark>();

	private UnityAction<RuneMark> runeSlotConfirmed;

	private UnityAction<RuneMark> runeSlotSelected;

	public Text naRuneText;

	public ScrollGroup scrollGroup;

	private void Awake()
	{
		scrollGroup = GetComponent<ScrollGroup>();
		if ((Object)(object)naRuneText != null)
		{
			((Component)(object)naRuneText).gameObject.SetActive(value: false);
		}
	}

	public void Clear()
	{
		items.Clear();
		runes.Clear();
		scrollGroup.ClearList();
	}

	public void SetList(List<Item> items, UnityAction<Item> slotConfirmed, UnityAction<Item> slotSelected, bool addEmpty, bool displayPrice, bool buy, bool flagSold = false, bool allowHighlight = true, string reason = null)
	{
		if ((Object)(object)naRuneText != null)
		{
			if (!string.IsNullOrEmpty(reason))
			{
				((Component)(object)naRuneText).gameObject.SetActive(value: true);
				naRuneText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(reason));
			}
			else
			{
				((Component)(object)naRuneText).gameObject.SetActive(value: false);
			}
		}
		Clear();
		scrollGroup.Setup(itemPrefab, hideBarIfEmpty: true);
		itemSlotConfirmed = slotConfirmed;
		itemSlotSelected = slotSelected;
		if (addEmpty)
		{
			AddItemToList(new Item(ItemId.NONE), displayPrice, buy, flagSold, allowHighlight);
		}
		for (int i = 0; i < items.Count; i++)
		{
			AddItemToList(items[i], displayPrice, buy, flagSold, allowHighlight);
		}
		scrollGroup.RepositionScrollListOnNextFrame();
	}

	private void AddItemToList(Item item, bool displayPrice, bool buy, bool flagSold = false, bool allowHighlight = true)
	{
		GameObject gameObject = scrollGroup.AddToList(null, null);
		ToggleEffects component = gameObject.GetComponent<ToggleEffects>();
		int idx = items.Count;
		((UnityEvent<bool>)(object)component.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			if (isOn && itemSlotSelected != null)
			{
				itemSlotSelected(item);
			}
		});
		component.onAction.AddListener(delegate
		{
			ItemConfirmed(idx);
		});
		if (!allowHighlight)
		{
			component.toggleOnSelect = false;
			component.toggleOnOver = false;
			component.highlightOnSelect = false;
			component.highlightOnOver = false;
		}
		UIInventoryItem component2 = gameObject.GetComponent<UIInventoryItem>();
		component2.Set(item, displayPrice, buy, ItemId.NONE, flagSold);
		items.Add(item);
	}

	public void RemoveItem(Item item)
	{
		int num = 0;
		UIInventoryItem component;
		while (true)
		{
			if (num < scrollGroup.items.Count)
			{
				component = scrollGroup.items[num].GetComponent<UIInventoryItem>();
				if (component.item == item)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		if (!component.UpdateQuantity())
		{
			scrollGroup.RemoveItemAt(num);
		}
	}

	private void ItemConfirmed(int idx)
	{
		if (itemSlotConfirmed != null)
		{
			itemSlotConfirmed(items[idx]);
		}
	}

	public void SetList(List<RuneMark> availableRuneList, List<RuneMark> notAvailableRuneList, UnityAction<RuneMark> slotConfirmed, UnityAction<RuneMark> slotSelected, string reason = null)
	{
		if ((Object)(object)naRuneText != null)
		{
			if (!string.IsNullOrEmpty(reason))
			{
				((Component)(object)naRuneText).gameObject.SetActive(value: true);
				naRuneText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(reason));
			}
			else
			{
				((Component)(object)naRuneText).gameObject.SetActive(value: false);
			}
		}
		Clear();
		scrollGroup.Setup(runePrefab, hideBarIfEmpty: true);
		runeSlotConfirmed = slotConfirmed;
		runeSlotSelected = slotSelected;
		for (int i = 0; i < availableRuneList.Count; i++)
		{
			AddRuneToList(availableRuneList[i], hasRecipe: true);
		}
		for (int j = 0; j < notAvailableRuneList.Count; j++)
		{
			AddRuneToList(notAvailableRuneList[j], hasRecipe: false);
		}
		scrollGroup.RepositionScrollListOnNextFrame();
	}

	private void AddRuneToList(RuneMark rune, bool hasRecipe)
	{
		GameObject gameObject = scrollGroup.AddToList(null, null);
		ToggleEffects component = gameObject.GetComponent<ToggleEffects>();
		int idx = runes.Count;
		((UnityEvent<bool>)(object)component.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			if (isOn)
			{
				runeSlotSelected(rune);
			}
		});
		if (hasRecipe)
		{
			component.onAction.AddListener(delegate
			{
				RuneConfirmed(idx);
			});
		}
		UIInventoryRune component2 = gameObject.GetComponent<UIInventoryRune>();
		component2.Set(rune);
		runes.Add(rune);
	}

	private void RuneConfirmed(int idx)
	{
		runeSlotConfirmed(runes[idx]);
	}

	public void SetFocus()
	{
		if (scrollGroup != null && scrollGroup.items.Count > 0)
		{
			scrollGroup.items[0].SetSelected(force: true);
		}
	}
}
