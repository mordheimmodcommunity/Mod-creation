using System.Collections.Generic;

public class Chest
{
	protected List<ItemSave> items;

	public Chest()
	{
		items = new List<ItemSave>();
	}

	public Chest(List<ItemSave> list)
	{
		items = list;
		ForceRecipeToNormalQuality();
	}

	public void ForceRecipeToNormalQuality()
	{
		List<RuneMarkRecipeData> list = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkRecipeData>();
		for (int i = 0; i < items.Count; i++)
		{
			ItemSave itemSave = items[i];
			if (itemSave.qualityId > 1)
			{
				ItemId itemId = (ItemId)itemSave.id;
				if (list.Exists((RuneMarkRecipeData x) => x.ItemId == itemId))
				{
					itemSave.qualityId = 1;
				}
			}
		}
	}

	public bool AddItem(ItemSave save, bool sold = false)
	{
		return AddItem((ItemId)save.id, (ItemQualityId)save.qualityId, (RuneMarkId)save.runeMarkId, (RuneMarkQualityId)save.runeMarkQualityId, (AllegianceId)save.allegianceId, save.amount, sold: true);
	}

	public virtual bool AddItem(ItemId itemId, ItemQualityId quality = ItemQualityId.NORMAL, RuneMarkId runeMark = RuneMarkId.NONE, RuneMarkQualityId runeMarkQuality = RuneMarkQualityId.NONE, AllegianceId allegiance = AllegianceId.NONE, int count = 1, bool sold = false)
	{
		if (itemId == ItemId.NONE)
		{
			return false;
		}
		ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)itemId);
		if (itemData.Backup)
		{
			return false;
		}
		for (int i = 0; i < items.Count; i++)
		{
			ItemSave itemSave = items[i];
			if (itemSave.id == (int)itemId && itemSave.qualityId == (int)quality && itemSave.runeMarkId == (int)runeMark && itemSave.runeMarkQualityId == (int)runeMarkQuality)
			{
				itemSave.amount += count;
				if (sold)
				{
					itemSave.soldAmount += count;
				}
				return true;
			}
		}
		ItemSave itemSave2 = new ItemSave(itemId, quality, runeMark, runeMarkQuality, allegiance, count);
		if (sold)
		{
			itemSave2.soldAmount = count;
		}
		items.Add(itemSave2);
		return true;
	}

	public bool AddItems(List<Item> from)
	{
		bool flag = false;
		for (int i = 0; i < from.Count; i++)
		{
			flag |= AddItem(from[i].Save);
		}
		return flag;
	}

	public bool AddItems(List<ItemSave> from)
	{
		bool flag = false;
		for (int i = 0; i < from.Count; i++)
		{
			flag |= AddItem(from[i]);
		}
		return flag;
	}

	public ItemSave PopItem(ItemSave save, int qty = 1)
	{
		if (RemoveItem(save, qty))
		{
			return new ItemSave((ItemId)save.id, (ItemQualityId)save.qualityId, (RuneMarkId)save.runeMarkId, (RuneMarkQualityId)save.runeMarkQualityId, (AllegianceId)save.allegianceId, qty);
		}
		return null;
	}

	public void Clear()
	{
		items.Clear();
	}

	public bool RemoveItem(ItemId itemId, int count)
	{
		return RemoveItem(itemId, ItemQualityId.NORMAL, RuneMarkId.NONE, RuneMarkQualityId.NONE, AllegianceId.NONE, count);
	}

	public bool RemoveItem(ItemSave save, int qty = 1)
	{
		return RemoveItem((ItemId)save.id, (ItemQualityId)save.qualityId, (RuneMarkId)save.runeMarkId, (RuneMarkQualityId)save.runeMarkQualityId, (AllegianceId)save.allegianceId, qty);
	}

	public virtual bool RemoveItem(ItemId itemId, ItemQualityId quality = ItemQualityId.NORMAL, RuneMarkId runeMark = RuneMarkId.NONE, RuneMarkQualityId runeMarkQuality = RuneMarkQualityId.NONE, AllegianceId allegiance = AllegianceId.NONE, int count = 1)
	{
		if (itemId == ItemId.NONE)
		{
			return false;
		}
		for (int i = 0; i < items.Count; i++)
		{
			ItemSave itemSave = items[i];
			if (itemSave.id == (int)itemId && itemSave.qualityId == (int)quality && itemSave.runeMarkId == (int)runeMark && itemSave.runeMarkQualityId == (int)runeMarkQuality)
			{
				if (count > itemSave.amount)
				{
					count = itemSave.amount;
				}
				itemSave.amount -= count;
				if (itemSave.amount <= 0)
				{
					items.RemoveAt(i);
				}
				return true;
			}
		}
		return false;
	}

	public bool HasItem(Item item)
	{
		return HasItem(item.Id, item.QualityData.Id, (item.RuneMark != null) ? item.RuneMark.Data.Id : RuneMarkId.NONE, (item.RuneMark != null) ? item.RuneMark.QualityData.Id : RuneMarkQualityId.NONE);
	}

	public bool HasItem(ItemSave save)
	{
		return HasItem((ItemId)save.id, (ItemQualityId)save.qualityId, (RuneMarkId)save.runeMarkId, (RuneMarkQualityId)save.runeMarkQualityId);
	}

	public bool HasItem(ItemId itemId, ItemQualityId qualityId, RuneMarkId runeId = RuneMarkId.NONE, RuneMarkQualityId runeQualityId = RuneMarkQualityId.NONE)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].id == (int)itemId && items[i].qualityId == (int)qualityId && items[i].runeMarkId == (int)runeId && items[i].runeMarkQualityId == (int)runeQualityId)
			{
				return true;
			}
		}
		return false;
	}

	public ItemSave GetItem(ItemId id, ItemQualityId qualityId = ItemQualityId.NORMAL)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].id == (int)id && items[i].qualityId == (int)qualityId)
			{
				return items[i];
			}
		}
		return new ItemSave(ItemId.NONE);
	}

	public virtual List<ItemSave> GetItems()
	{
		return items;
	}

	public void RemoveSoldItems()
	{
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].soldAmount == items[num].amount)
			{
				items.RemoveAt(num);
			}
			else
			{
				items[num].amount -= items[num].soldAmount;
				items[num].soldAmount = 0;
			}
		}
	}

	public virtual List<ItemSave> GetItems(Unit unit, UnitSlotId slotId)
	{
		List<ItemSave> list = new List<ItemSave>();
		slotId = ((slotId <= UnitSlotId.ITEM_1) ? slotId : UnitSlotId.ITEM_1);
		bool flag = false;
		if (slotId == UnitSlotId.SET1_OFFHAND || slotId == UnitSlotId.SET2_OFFHAND)
		{
			for (int i = 0; i < unit.Items[(int)(slotId - 1)].Enchantments.Count; i++)
			{
				if (unit.Items[(int)(slotId - 1)].Enchantments[i].Id == EnchantmentId.ITEM_UNWIELDY)
				{
					flag = true;
					break;
				}
			}
		}
		MutationId mutationId = unit.GetMutationId(slotId);
		bool flag2 = (slotId == UnitSlotId.SET1_MAINHAND || slotId == UnitSlotId.SET2_MAINHAND) && unit.GetMutationId(slotId + 1) != MutationId.NONE;
		bool flag3 = (slotId == UnitSlotId.SET1_MAINHAND || slotId == UnitSlotId.SET2_MAINHAND) && unit.GetInjury(slotId + 1) != InjuryId.NONE;
		List<ItemUnitData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemUnitData>(new string[2]
		{
			"fk_unit_id",
			"mutation"
		}, new string[2]
		{
			((int)unit.Id).ToConstantString(),
			(mutationId == MutationId.NONE) ? "0" : "1"
		});
		List<ItemId> list3 = new List<ItemId>();
		for (int j = 0; j < list2.Count; j++)
		{
			list3.Add(list2[j].ItemId);
		}
		if (slotId == UnitSlotId.SET2_MAINHAND || slotId == UnitSlotId.SET2_OFFHAND)
		{
			slotId -= 2;
		}
		for (int k = 0; k < items.Count; k++)
		{
			ItemSave itemSave = items[k];
			ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(itemSave.id);
			ItemTypeData itemTypeData = PandoraSingleton<DataFactory>.Instance.InitData<ItemTypeData>((int)itemData.ItemTypeId);
			List<ItemConsumableData> list4 = PandoraSingleton<DataFactory>.Instance.InitData<ItemConsumableData>(new string[2]
			{
				"fk_item_id",
				"fk_item_quality_id"
			}, new string[2]
			{
				itemSave.id.ToConstantString(),
				itemSave.qualityId.ToConstantString()
			});
			ItemConsumableData itemConsumableData = (list4 == null || list4.Count <= 0) ? null : list4[0];
			List<ItemEnchantmentData> list5 = PandoraSingleton<DataFactory>.Instance.InitData<ItemEnchantmentData>(new string[3]
			{
				"fk_item_id",
				"fk_item_quality_id",
				"fk_enchantment_id"
			}, new string[3]
			{
				itemSave.id.ToConstantString(),
				itemSave.qualityId.ToConstantString(),
				51.ToConstantString()
			});
			bool flag4 = list5.Count > 0;
			bool flag5 = true;
			if (itemConsumableData != null)
			{
				List<ItemConsumableLockConsumableData> list6 = PandoraSingleton<DataFactory>.Instance.InitData<ItemConsumableLockConsumableData>("fk_skill_id_locked", ((int)itemConsumableData.SkillId).ToConstantString());
				for (int l = 0; l < list6.Count; l++)
				{
					if (unit.UnitSave.consumableSkills.IndexOf(list6[l].SkillId, SkillIdComparer.Instance) != -1)
					{
						flag5 = false;
						break;
					}
				}
			}
			List<ItemJoinUnitSlotData> list7 = PandoraSingleton<DataFactory>.Instance.InitData<ItemJoinUnitSlotData>("fk_item_id", itemData.Id.ToIntString());
			if (list7.Exists((ItemJoinUnitSlotData x) => x.UnitSlotId == slotId) && list3.IndexOf((ItemId)itemSave.id, ItemIdComparer.Instance) != -1 && !unit.IsItemTypeBlocked(itemTypeData.Id) && (!flag || (flag && itemData.ItemTypeId == ItemTypeId.SHIELD)) && (!flag2 || (!itemTypeData.IsTwoHanded && !itemData.Paired && !flag4)) && (!flag3 || (!itemTypeData.IsTwoHanded && !itemData.Paired)) && flag5)
			{
				list.Add(items[k]);
			}
		}
		return list;
	}
}
