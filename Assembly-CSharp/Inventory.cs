using System.Collections.Generic;

public class Inventory : ICheapState
{
	private UnitController unitCtrlr;

	private int itemIndex;

	private int slotIndex;

	private readonly List<Item> unitItems = new List<Item>();

	public Inventory(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		itemIndex = 0;
		slotIndex = 0;
		UpdateInventory();
		SearchPoint searchPoint = (SearchPoint)unitCtrlr.interactivePoint;
		unitCtrlr.GetWarband().LocateItem(searchPoint.items);
		if (unitCtrlr.IsPlayed())
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_INVENTORY);
		}
	}

	void ICheapState.Exit(int iTo)
	{
		unitItems.Clear();
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	public void UpdateInventory()
	{
		unitItems.Clear();
		for (int i = 6; i < unitCtrlr.unit.Items.Count; i++)
		{
			unitItems.Add(unitCtrlr.unit.Items[i]);
		}
	}

	private void SwitchItem()
	{
		SearchPoint searchPoint = (SearchPoint)unitCtrlr.interactivePoint;
		Item item = unitItems[slotIndex];
		Item item2 = null;
		if (itemIndex == -1)
		{
			searchPoint.AddItem(item);
			item2 = new Item(ItemId.NONE);
			unitCtrlr.unit.EquipItem((UnitSlotId)(6 + slotIndex), item2);
		}
		else
		{
			Item item3 = searchPoint.GetItems()[itemIndex];
			item2 = searchPoint.SwitchItem(unitCtrlr, itemIndex, item);
			if (item2 != item)
			{
				unitCtrlr.unit.EquipItem((UnitSlotId)(6 + slotIndex), item2);
				WarbandController warband = unitCtrlr.GetWarband();
				if (item3.IsIdol)
				{
					unitCtrlr.unit.AddEnchantment(EnchantmentId.IDOL_THEFT, unitCtrlr.unit, original: false);
					unitCtrlr.Imprint.idolTexture = item3.GetIcon();
				}
				else if (item.IsIdol)
				{
					unitCtrlr.unit.RemoveEnchantment(EnchantmentId.IDOL_THEFT, unitCtrlr.unit);
					unitCtrlr.Imprint.idolTexture = null;
					for (int i = 6; i < unitCtrlr.unit.Items.Count; i++)
					{
						if (unitCtrlr.unit.Items[i].IsIdol)
						{
							unitCtrlr.Imprint.idolTexture = unitCtrlr.unit.Items[i].GetIcon();
							break;
						}
					}
				}
				for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; j++)
				{
					if (item3.IsIdol && item3 == PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].ItemIdol)
					{
						if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].teamIdx == warband.teamIdx)
						{
							PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].AddMoralIdol();
						}
						else
						{
							PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].RemoveMoralIdol();
						}
					}
					else if (item.IsIdol && item == PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].ItemIdol)
					{
						if (searchPoint == PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].wagon.idol)
						{
							PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].AddMoralIdol();
						}
						else if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].teamIdx == warband.teamIdx)
						{
							PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].AddMoralIdol();
						}
						else
						{
							PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[j].RemoveMoralIdol();
						}
					}
				}
			}
		}
		UpdateInventory();
	}

	public void PickupItem(int inventoryIndex, int searchZoneSlotIndex)
	{
		UpdateInventory();
		itemIndex = inventoryIndex;
		slotIndex = searchZoneSlotIndex;
		SwitchItem();
	}

	public void CloseInventory()
	{
		unitCtrlr.SendInventoryDone();
	}
}
