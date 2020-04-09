using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryController : CanvasGroupDisabler
{
	private const PandoraInput.InputLayer uiLayer = PandoraInput.InputLayer.LOOTING;

	private static Item NONE_ITEM;

	public GameObject itemPrefab;

	public UIInventoryItem centerItem;

	public UIInventoryItemDescription centerItemDescription;

	public Text titleText;

	public Text subtitleText;

	public Image titleIcon;

	public UIInventoryGroup leftGroup;

	public UIInventoryGroup rightGroup;

	public List<ButtonGroup> buttons;

	private SearchPoint currentSearchPoint;

	private bool isShow;

	public Sprite overlayEnemy;

	public Sprite overlayAlly;

	public Sprite overlayNeutral;

	private bool isLeft = true;

	private bool mustReset;

	private void Awake()
	{
		if (NONE_ITEM == null)
		{
			NONE_ITEM = new Item(ItemId.NONE);
		}
	}

	private bool CanTakeAll()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < currentSearchPoint.items.Count; i++)
		{
			if (currentSearchPoint.items[i].Id == ItemId.NONE || !currentSearchPoint.CanSwitchItem(i, NONE_ITEM))
			{
				continue;
			}
			if (currentSearchPoint.items[i].IsStackable)
			{
				ItemId itemId = currentSearchPoint.items[i].Id;
				int num3 = PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.Items.FindIndex((Item x) => x.Id == itemId);
				if (num3 >= 0)
				{
					num++;
				}
			}
			else
			{
				num2++;
			}
		}
		return num2 <= PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.GetNumEmptyItemSlot();
	}

	private void TakeAll()
	{
		OnDisable();
		PandoraSingleton<MissionManager>.Instance.focusedUnit.SendInventoryTakeAll();
	}

	private List<Item> GetInventoryItems()
	{
		return PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.Items.GetRange(6, PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.BackpackCapacity);
	}

	public void Show()
	{
		PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: true);
		isShow = true;
		subtitleText.set_text(string.Empty);
		currentSearchPoint = (SearchPoint)PandoraSingleton<MissionManager>.Instance.focusedUnit.interactivePoint;
		bool requiredItem = currentSearchPoint.HasRequiredItem();
		PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.LOOTING);
		base.gameObject.SetActive(value: true);
		leftGroup.Setup(PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.Name, PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.GetIcon(), itemPrefab, OnChooseInventoryItem, PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.BackpackCapacity, overlayAlly, ShowDescriptionInventory);
		string locAction = currentSearchPoint.GetLocAction();
		titleText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(locAction));
		titleIcon.set_sprite(currentSearchPoint.GetIconAction());
		if (currentSearchPoint.unitController != null)
		{
			rightGroup.Setup(PandoraSingleton<LocalizationManager>.Instance.GetStringById(currentSearchPoint.loc_name, currentSearchPoint.unitController.unit.Name), currentSearchPoint.unitController.unit.GetIcon(), itemPrefab, OnChooseSearchItem, currentSearchPoint.items.Count, (!PandoraSingleton<MissionManager>.Instance.focusedUnit.IsMine()) ? overlayEnemy : overlayAlly, ShowDescriptionSearchPoint, requiredItem);
		}
		else
		{
			WarbandController myWarbandCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
			if (currentSearchPoint.warbandController == null)
			{
				rightGroup.Setup(PandoraSingleton<LocalizationManager>.Instance.GetStringById(currentSearchPoint.loc_name), titleIcon.get_sprite(), itemPrefab, OnChooseSearchItem, currentSearchPoint.items.Count, overlayNeutral, ShowDescriptionSearchPoint, requiredItem);
			}
			else if (myWarbandCtrlr == currentSearchPoint.warbandController)
			{
				rightGroup.Setup(PandoraSingleton<LocalizationManager>.Instance.GetStringById(currentSearchPoint.loc_name, currentSearchPoint.warbandController.name), Warband.GetIcon(currentSearchPoint.warbandController.WarData.Id), itemPrefab, OnChooseSearchItem, currentSearchPoint.items.Count, overlayAlly, ShowDescriptionSearchPoint, requiredItem);
			}
			else
			{
				rightGroup.Setup(PandoraSingleton<LocalizationManager>.Instance.GetStringById(currentSearchPoint.loc_name, currentSearchPoint.warbandController.name), Warband.GetIcon(currentSearchPoint.warbandController.WarData.Id), itemPrefab, OnChooseSearchItem, currentSearchPoint.items.Count, overlayEnemy, ShowDescriptionSearchPoint, requiredItem);
			}
		}
		Reset();
	}

	private void Close()
	{
		OnDisable();
		Inventory inventory = (Inventory)PandoraSingleton<MissionManager>.Instance.focusedUnit.StateMachine.GetActiveState();
		inventory.CloseInventory();
	}

	private void OnChooseInventoryItem(int index)
	{
		if (leftGroup.IsSwapping)
		{
			leftGroup.swapIndex = index;
			return;
		}
		if (rightGroup.IsSwapping)
		{
			Swap(index, rightGroup.swapIndex);
			leftGroup.uiItems[index].SetSelected(force: true);
			return;
		}
		int emptySlot = currentSearchPoint.GetEmptySlot();
		Item unitItem = PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.Items[6 + index];
		int num = currentSearchPoint.items.FindIndex((Item x) => x.Id == unitItem.Id);
		if (unitItem.IsStackable && num >= 0)
		{
			Swap(index, -1);
			return;
		}
		if (emptySlot != -1)
		{
			Swap(index, emptySlot);
			leftGroup.uiItems[index].SetSelected(force: true);
			return;
		}
		leftGroup.swapIndex = index;
		leftGroup.Lock();
		rightGroup.uiItems[0].GetComponent<ToggleEffects>().SetOn();
		buttons[0].SetAction("cancel", "loot_cancel_swap", 4);
		buttons[0].OnAction(Reset, mouseOnly: false);
		buttons[1].SetAction("action", "loot_swap", 4);
		buttons[2].gameObject.SetActive(value: false);
	}

	private void OnChooseSearchItem(int index)
	{
		if (leftGroup.IsSwapping)
		{
			Swap(leftGroup.swapIndex, index);
			Reset();
			return;
		}
		if (rightGroup.IsSwapping)
		{
			rightGroup.swapIndex = index;
			return;
		}
		if (PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.GetEmptyItemSlot(out UnitSlotId slotId, currentSearchPoint.items[index]))
		{
			Swap((int)(slotId - 6), index);
			return;
		}
		rightGroup.Lock();
		rightGroup.swapIndex = index;
		leftGroup.uiItems[0].GetComponent<ToggleEffects>().SetOn();
		buttons[0].SetAction("cancel", "loot_cancel_swap", 4);
		buttons[0].OnAction(Reset, mouseOnly: false);
		buttons[1].SetAction("action", "loot_swap", 4);
		buttons[2].gameObject.SetActive(value: false);
	}

	private void Reset()
	{
		leftGroup.Unlock();
		rightGroup.Unlock();
		leftGroup.swapIndex = -1;
		rightGroup.swapIndex = -1;
		leftGroup.Refresh(GetInventoryItems());
		rightGroup.Refresh(currentSearchPoint.items, currentSearchPoint.GetRestrictedItemIds());
		buttons[0].SetAction("cancel", "loot_quit", 4);
		buttons[0].OnAction(Close, mouseOnly: false);
		buttons[1].SetAction("action", "loot_take", 4);
		if (!currentSearchPoint.IsEmpty() && CanTakeAll())
		{
			buttons[2].SetAction("take_all", "loot_take_all", 4);
			buttons[2].OnAction(TakeAll, mouseOnly: false);
		}
		else
		{
			buttons[2].gameObject.SetActive(value: false);
		}
		buttons[3].gameObject.SetActive(value: false);
		rightGroup.scrollGroup.ResetSelection();
		rightGroup.uiItems[0].GetComponent<ToggleEffects>().SetOn();
	}

	private void Swap(int inventoryIndex, int searchIndex)
	{
		PandoraSingleton<MissionManager>.Instance.focusedUnit.SendInventoryChange(searchIndex, inventoryIndex);
		StartCoroutine(WaitToReset());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (isShow)
		{
			isShow = false;
			PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.LOOTING);
			PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void ShowDescriptionInventory(Item item)
	{
		isLeft = true;
		if (!rightGroup.IsSwapping)
		{
			rightGroup.toggleGroup.SetAllTogglesOff();
		}
		ShowDescription(item);
	}

	private void ShowDescriptionSearchPoint(Item item)
	{
		isLeft = false;
		if (!leftGroup.IsSwapping)
		{
			leftGroup.toggleGroup.SetAllTogglesOff();
		}
		ShowDescription(item);
	}

	private void ShowDescription(Item item)
	{
		if (item.Id == ItemId.NONE)
		{
			centerItem.gameObject.SetActive(value: false);
			return;
		}
		centerItem.gameObject.SetActive(value: true);
		centerItem.Set(item);
		centerItemDescription.Set(item);
	}

	private void Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h", 4))
		{
			if (isLeft && !rightGroup.IsSwapping)
			{
				rightGroup.uiItems[0].SetSelected(force: true);
			}
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h", 4) && !isLeft && !leftGroup.IsSwapping)
		{
			leftGroup.uiItems[0].SetSelected(force: true);
		}
	}

	private IEnumerator WaitToReset()
	{
		yield return null;
		yield return new WaitForEndOfFrame();
		Reset();
	}
}
