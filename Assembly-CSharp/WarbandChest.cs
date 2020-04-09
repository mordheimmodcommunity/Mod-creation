using System.Collections.Generic;

public class WarbandChest : Chest
{
	public Warband warband;

	public bool sendStats = true;

	public WarbandChest()
	{
		warband = null;
	}

	public WarbandChest(Warband wb)
		: base(wb.GetWarbandSave().items)
	{
		warband = wb;
		ItemSave itemSave = PopItem(new ItemSave(ItemId.WYRDSTONE_FRAGMENT, ItemQualityId.NONE));
		if (itemSave != null)
		{
			itemSave.qualityId = 1;
			AddItem(itemSave);
		}
		itemSave = PopItem(new ItemSave(ItemId.WYRDSTONE_SHARD, ItemQualityId.NONE));
		if (itemSave != null)
		{
			itemSave.qualityId = 1;
			AddItem(itemSave);
		}
		itemSave = PopItem(new ItemSave(ItemId.WYRDSTONE_CLUSTER, ItemQualityId.NONE));
		if (itemSave != null)
		{
			itemSave.qualityId = 1;
			AddItem(itemSave);
		}
	}

	public override bool AddItem(ItemId itemId, ItemQualityId quality = ItemQualityId.NORMAL, RuneMarkId runeMark = RuneMarkId.NONE, RuneMarkQualityId runeMarkQuality = RuneMarkQualityId.NONE, AllegianceId allegiance = AllegianceId.NONE, int count = 1, bool sold = false)
	{
		bool flag = base.AddItem(itemId, quality, runeMark, runeMarkQuality, allegiance, count);
		if (flag)
		{
			switch (itemId)
			{
			case ItemId.GOLD:
				warband.AddToAttribute(WarbandAttributeId.TOTAL_GOLD, count);
				if (sendStats)
				{
					PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.GOLD_EARNED, count);
				}
				break;
			case ItemId.WYRDSTONE_FRAGMENT:
				warband.AddToAttribute(WarbandAttributeId.FRAGMENTS_GATHERED, count);
				break;
			case ItemId.WYRDSTONE_SHARD:
				warband.AddToAttribute(WarbandAttributeId.SHARDS_GATHERED, count);
				break;
			case ItemId.WYRDSTONE_CLUSTER:
				warband.AddToAttribute(WarbandAttributeId.CLUSTERS_GATHERED, count);
				break;
			}
			ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)itemId);
			if (itemData.ItemTypeId == ItemTypeId.RECIPE_ENCHANTMENT_NORMAL || itemData.ItemTypeId == ItemTypeId.RECIPE_ENCHANTMENT_MASTERY)
			{
				warband.AddToAttribute(WarbandAttributeId.RECIPE_FOUND, 1);
				PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.UNLOCKED_RECIPES, 1);
			}
		}
		return flag;
	}

	public int GetGold()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].id == 133)
			{
				return items[i].amount;
			}
		}
		return 0;
	}

	public void RemoveGold(int amount)
	{
		int num = 0;
		while (true)
		{
			if (num < items.Count)
			{
				if (items[num].id == 133)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		items[num].amount -= amount;
	}

	public void AddGold(int amount)
	{
		warband.AddToAttribute(WarbandAttributeId.TOTAL_GOLD, amount);
		int num = 0;
		while (true)
		{
			if (num < items.Count)
			{
				if (items[num].id == 133)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		items[num].amount += amount;
	}

	public List<ItemSave> GetSellableItems()
	{
		List<ItemSave> list = new List<ItemSave>();
		for (int i = 0; i < items.Count; i++)
		{
			ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(items[i].id);
			if (itemData.Sellable)
			{
				list.Add(items[i]);
			}
		}
		return list;
	}
}
