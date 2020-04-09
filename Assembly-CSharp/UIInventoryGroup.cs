using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryGroup : CanvasGroupDisabler
{
	public Image titleIcon;

	public Image titleBackground;

	public Text titleText;

	public Text subtitleText;

	public ScrollGroup scrollGroup;

	public readonly List<UIInventoryItem> uiItems = new List<UIInventoryItem>();

	private List<Item> items = new List<Item>();

	public int selectedIndex = -1;

	public int swapIndex = -1;

	private Action<int> itemSelectedCallback;

	private Action<Item> showDescriptionCallback;

	private HightlightAnimate hightlight;

	public ToggleGroup toggleGroup;

	public bool IsSwapping => swapIndex != -1;

	private void Awake()
	{
		hightlight = GetComponentInChildren<HightlightAnimate>();
		toggleGroup = GetComponentInParent<ToggleGroup>();
	}

	public void Setup(string name, Sprite icon, GameObject itemPrefab, Action<int> itemSelected, int nb, Sprite background, Action<Item> showDescription, bool requiredItem = false)
	{
		titleBackground.set_sprite(background);
		itemSelectedCallback = itemSelected;
		titleText.set_text(name);
		subtitleText.set_text((!requiredItem) ? string.Empty : PandoraSingleton<LocalizationManager>.Instance.GetStringById("required_item"));
		titleIcon.set_sprite(icon);
		scrollGroup.ClearList();
		scrollGroup.Setup(itemPrefab, hideBarIfEmpty: true);
		uiItems.Clear();
		for (int i = 0; i < nb; i++)
		{
			GameObject gameObject = scrollGroup.AddToList(null, null);
			uiItems.Add(gameObject.GetComponent<UIInventoryItem>());
			int index = i;
			ToggleEffects component = gameObject.GetComponent<ToggleEffects>();
			component.onSelect.AddListener(delegate
			{
				OnOverItem(index);
			});
			component.onAction.AddListener(delegate
			{
				itemSelectedCallback(index);
			});
		}
		showDescriptionCallback = showDescription;
	}

	public void OnOverItem(int index)
	{
		selectedIndex = index;
		showDescriptionCallback(items[index]);
	}

	public void Refresh(List<Item> refItems, List<ItemId> restrictedItemIds = null)
	{
		selectedIndex = -1;
		items = refItems;
		for (int i = 0; i < items.Count; i++)
		{
			uiItems[i].Set(items[i], shop: false, buy: false, restrictedItemIds?[i] ?? ItemId.NONE);
		}
		toggleGroup.SetAllTogglesOff();
		hightlight.Deactivate();
	}

	public void Lock()
	{
		CanvasGroup canvasGroup = base.CanvasGroup;
		bool flag = false;
		base.CanvasGroup.interactable = flag;
		canvasGroup.blocksRaycasts = flag;
	}

	public void Unlock()
	{
		CanvasGroup canvasGroup = base.CanvasGroup;
		bool flag = true;
		base.CanvasGroup.interactable = flag;
		canvasGroup.blocksRaycasts = flag;
	}
}
