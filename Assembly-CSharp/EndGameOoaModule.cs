using System.Collections.Generic;
using UnityEngine;

public class EndGameOoaModule : UIModule
{
	public GameObject costOfLosing;

	public UIDescription titleDesc;

	public ListGroup mainWeapons;

	public ListGroup secWeapons;

	public ListGroup equipment;

	public ListGroup consumables;

	public ListGroup chestItems;

	public UIDescription injuryItem;

	public GameObject item;

	public CostOfLosingData col;

	public void Set(MissionEndUnitSave endUnit, Unit unit)
	{
		mainWeapons.ClearList();
		secWeapons.ClearList();
		equipment.ClearList();
		consumables.ClearList();
		costOfLosing.SetActive(value: true);
		titleDesc.Set("end_game_items_lost", "end_game_items_lost_desc");
		mainWeapons.gameObject.SetActive(value: false);
		secWeapons.gameObject.SetActive(value: false);
		equipment.gameObject.SetActive(value: false);
		consumables.gameObject.SetActive(value: false);
		injuryItem.gameObject.SetActive(value: false);
		PandoraDebug.LogDebug("Cost Of Losing Id = " + endUnit.costOfLosingId);
		bool flag = false;
		if (endUnit.costOfLosingId > 1)
		{
			col = PandoraSingleton<DataFactory>.Instance.InitData<CostOfLosingData>(endUnit.costOfLosingId);
			if (endUnit.lostItems[2].Id != 0)
			{
				flag = true;
				mainWeapons.Setup("combat_main_weapon_set", item);
				GameObject gameObject = mainWeapons.AddToList();
				UIInventoryItem component = gameObject.GetComponent<UIInventoryItem>();
				component.Set(endUnit.lostItems[2]);
				if (endUnit.lostItems[3].Id != 0)
				{
					gameObject = mainWeapons.AddToList();
					component = gameObject.GetComponent<UIInventoryItem>();
					component.Set(unit.Items[3]);
				}
				showQueue.Enqueue(mainWeapons.gameObject);
			}
			if (endUnit.lostItems[4].Id != 0)
			{
				flag = true;
				secWeapons.Setup("combat_alternate_weapon_set", item);
				GameObject gameObject2 = secWeapons.AddToList();
				UIInventoryItem component2 = gameObject2.GetComponent<UIInventoryItem>();
				component2.Set(endUnit.lostItems[4]);
				if (endUnit.lostItems[5].Id != 0)
				{
					gameObject2 = secWeapons.AddToList();
					component2 = gameObject2.GetComponent<UIInventoryItem>();
					component2.Set(unit.Items[5]);
				}
				showQueue.Enqueue(secWeapons.gameObject);
			}
			if (endUnit.lostItems[1].Id != 0 || endUnit.lostItems[0].Id != 0)
			{
				flag = true;
				equipment.Setup("hideout_menu_unit_equipment", item);
				if (endUnit.lostItems[0].Id != 0)
				{
					GameObject gameObject3 = equipment.AddToList();
					UIInventoryItem component3 = gameObject3.GetComponent<UIInventoryItem>();
					component3.Set(endUnit.lostItems[0]);
				}
				if (endUnit.lostItems[1].Id != 0)
				{
					GameObject gameObject4 = equipment.AddToList();
					UIInventoryItem component4 = gameObject4.GetComponent<UIInventoryItem>();
					component4.Set(endUnit.lostItems[1]);
				}
				showQueue.Enqueue(equipment.gameObject);
			}
			consumables.Setup("menu_backpack", item);
			for (int i = 6; i < unit.Items.Count; i++)
			{
				if (i < endUnit.lostItems.Count && endUnit.lostItems[i].Id != 0)
				{
					GameObject gameObject5 = consumables.AddToList();
					UIInventoryItem component5 = gameObject5.GetComponent<UIInventoryItem>();
					component5.Set(unit.Items[i]);
				}
			}
			if (consumables.items.Count > 0)
			{
				flag = true;
				showQueue.Enqueue(consumables.gameObject);
			}
			if (col.OpenWound)
			{
				flag = true;
				injuryItem.Set("enchant_title_open_wound", "enchant_desc_open_wound");
				injuryItem.gameObject.SetActive(value: false);
				showQueue.Enqueue(injuryItem.gameObject);
			}
		}
		if (!flag)
		{
			injuryItem.Set("end_game_safe_return_title", "end_game_safe_return_desc");
			showQueue.Enqueue(injuryItem.gameObject);
		}
		StartShow(0.5f);
	}

	public void Set(Chest chest)
	{
		costOfLosing.SetActive(value: true);
		chestItems.gameObject.SetActive(value: true);
		mainWeapons.gameObject.SetActive(value: false);
		secWeapons.gameObject.SetActive(value: false);
		equipment.gameObject.SetActive(value: false);
		consumables.gameObject.SetActive(value: false);
		injuryItem.gameObject.SetActive(value: false);
		mainWeapons.ClearList();
		secWeapons.ClearList();
		equipment.ClearList();
		consumables.ClearList();
		chestItems.ClearList();
		chestItems.Setup(string.Empty, item);
		titleDesc.Set("warband_looted_items", "end_game_looted_items_desc");
		showQueue.Enqueue(chestItems.gameObject);
		chestItems.gameObject.SetActive(value: false);
		List<ItemSave> items = chest.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			GameObject gameObject = chestItems.AddToList();
			UIInventoryItem component = gameObject.GetComponent<UIInventoryItem>();
			component.Set(new Item(items[i]));
			showQueue.Enqueue(gameObject);
			gameObject.SetActive(value: false);
		}
		chestItems.gameObject.SetActive(value: false);
		StartShow(0.5f);
	}
}
