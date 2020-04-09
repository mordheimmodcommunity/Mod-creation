using System.Collections.Generic;

public class Market
{
	private Chest shop;

	private Chest addedItems;

	private Warband warband;

	public MarketEventId CurrentEventId
	{
		get;
		private set;
	}

	public Market(Warband wb)
	{
		warband = wb;
		SetCurrentEvent((MarketEventId)wb.GetWarbandSave().marketEventId);
		shop = new Chest(wb.GetWarbandSave().marketItems);
		addedItems = new Chest(wb.GetWarbandSave().addedMarketItems);
	}

	public List<ItemSave> GetItems()
	{
		AssertMarketValidity();
		return shop.GetItems();
	}

	public List<ItemSave> GetItems(Unit unit, UnitSlotId slotId)
	{
		AssertMarketValidity();
		return shop.GetItems(unit, slotId);
	}

	public List<ItemSave> GetAddedItems()
	{
		AssertMarketValidity();
		return addedItems.GetItems();
	}

	public bool HasItem(Item tempItem)
	{
		return shop.HasItem(tempItem);
	}

	public bool HasItem(ItemId itemId, ItemQualityId qualityId)
	{
		return shop.HasItem(itemId, qualityId);
	}

	public ItemSave PopItem(ItemSave save, int qty = 1)
	{
		return shop.PopItem(save, qty);
	}

	public void AddSoldItem(ItemSave save)
	{
		if (shop.GetItems().Count < Constant.GetInt(ConstantId.MAX_SHOP_ITEM) || shop.HasItem(save))
		{
			shop.AddItem(save, sold: true);
		}
	}

	private void AssertMarketValidity()
	{
	}

	private void SetCurrentEvent(MarketEventId eventId)
	{
		CurrentEventId = eventId;
		warband.GetWarbandSave().marketEventId = (int)CurrentEventId;
		warband.UpdateAttributes();
	}

	public void RefreshMarket(MarketEventId eventId = MarketEventId.NONE, bool announceRefresh = true)
	{
		Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		List<MarketEventData> list = PandoraSingleton<DataFactory>.Instance.InitData<MarketEventData>().ToDynList();
		if (eventId == MarketEventId.NONE)
		{
			if (!warband.HasUnclaimedRecipe(ItemId.RECIPE_RANDOM_NORMAL) && !warband.HasUnclaimedRecipe(ItemId.RECIPE_RANDOM_MASTERY))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Id == MarketEventId.EXOTIC_PIECE)
					{
						list.RemoveAt(i);
						break;
					}
				}
			}
			MarketEventData randomRatio = MarketEventData.GetRandomRatio(list, localTyche, warband.MarketEventModifiers);
			eventId = randomRatio.Id;
		}
		SetCurrentEvent(eventId);
		List<ItemId> allowedItemIds = warband.GetAllowedItemIds();
		addedItems.Clear();
		shop.RemoveSoldItems();
		List<ItemSave> items = shop.GetItems();
		int @int = Constant.GetInt(ConstantId.MAX_SHOP_ITEM);
		if (items.Count > @int)
		{
			List<ItemSave> list2 = new List<ItemSave>();
			List<ItemSave> list3 = new List<ItemSave>();
			List<ItemSave> list4 = new List<ItemSave>();
			for (int j = 0; j < items.Count; j++)
			{
				switch (items[j].qualityId)
				{
				case 1:
					list2.Add(items[j]);
					break;
				case 2:
					list3.Add(items[j]);
					break;
				case 3:
					list4.Add(items[j]);
					break;
				}
			}
			List<ItemSave> list5 = null;
			while (items.Count > @int)
			{
				if (list2.Count > 0)
				{
					list5 = list2;
				}
				else if (list3.Count > 0)
				{
					list5 = list3;
				}
				else if (list4.Count > 0)
				{
					list5 = list4;
				}
				int index = localTyche.Rand(0, list5.Count);
				items.Remove(list5[index]);
				list5.RemoveAt(index);
			}
		}
		if (CurrentEventId != MarketEventId.NO_ROTATION)
		{
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if ((float)localTyche.Rand(0, 100) >= Constant.GetFloat(ConstantId.MARKET_REMOVE_ITEM_RATIO) * 100f || items[num].amount <= 0)
				{
					items.RemoveAt(num);
				}
			}
			List<MarketRefillData> list6 = PandoraSingleton<DataFactory>.Instance.InitData<MarketRefillData>("fk_warband_rank_id", warband.Rank.ToString());
			for (int k = 0; k < list6.Count; k++)
			{
				List<ItemId> marketAdditionalItems = warband.GetMarketAdditionalItems(list6[k].ItemCategoryId);
				if (list6[k].ItemCategoryId == ItemCategoryId.CONSUMABLE_OUT_COMBAT && marketAdditionalItems.Count == 0)
				{
					continue;
				}
				int num2 = localTyche.Rand(list6[k].QuantityMin, list6[k].QuantityMax + 1);
				List<MarketRefillQualityData> datas = PandoraSingleton<DataFactory>.Instance.InitData<MarketRefillQualityData>(new string[2]
				{
					"fk_warband_rank_id",
					"fk_item_category_id"
				}, new string[2]
				{
					warband.Rank.ToString(),
					((int)list6[k].ItemCategoryId).ToString()
				});
				for (int l = 0; l < num2; l++)
				{
					MarketRefillQualityData randomRatio2 = MarketRefillQualityData.GetRandomRatio(datas, PandoraSingleton<GameManager>.Instance.LocalTyche);
					if (randomRatio2 != null)
					{
						ItemSave save = Item.GetRandomLootableItem(PandoraSingleton<GameManager>.Instance.LocalTyche, list6[k].ItemCategoryId, randomRatio2.ItemQualityId, randomRatio2.RuneMarkQualityId, warband.WarbandData.AllegianceId, allowedItemIds, marketAdditionalItems).Save;
						shop.AddItem(save);
						addedItems.AddItem(save);
					}
					else
					{
						PandoraDebug.LogWarning("No quality data found for this rank " + warband.Rank.ToString() + " and item category " + list6[k].ItemCategoryId, "MARKET");
					}
				}
			}
			MarketEventId currentEventId = CurrentEventId;
			if (currentEventId == MarketEventId.MASTER_PIECE || currentEventId == MarketEventId.EXOTIC_PIECE)
			{
				List<MarketPieceData> list7 = PandoraSingleton<DataFactory>.Instance.InitData<MarketPieceData>(new string[2]
				{
					"fk_market_event_id",
					"fk_warband_rank_id"
				}, new string[2]
				{
					((int)CurrentEventId).ToString(),
					warband.Rank.ToString()
				}).ToDynList();
				if (!warband.HasUnclaimedRecipe(ItemId.RECIPE_RANDOM_NORMAL))
				{
					for (int m = 0; m < list7.Count; m++)
					{
						if (list7[m].ItemCategoryId == ItemCategoryId.RECIPE_ENCHANTMENT_NORMAL)
						{
							list7.RemoveAt(m);
							break;
						}
					}
				}
				if (!warband.HasUnclaimedRecipe(ItemId.RECIPE_RANDOM_MASTERY))
				{
					for (int n = 0; n < list7.Count; n++)
					{
						if (list7[n].ItemCategoryId == ItemCategoryId.RECIPE_ENCHANTMENT_MASTERY)
						{
							list7.RemoveAt(n);
							break;
						}
					}
				}
				if (list7.Count > 0)
				{
					MarketPieceData randomRatio3 = MarketPieceData.GetRandomRatio(list7, PandoraSingleton<GameManager>.Instance.LocalTyche);
					ItemId excludedItemId = ItemId.NONE;
					for (int num3 = 0; num3 < 2; num3++)
					{
						ItemSave itemSave = null;
						itemSave = ((randomRatio3.ItemCategoryId == ItemCategoryId.RECIPE_ENCHANTMENT_NORMAL) ? warband.GetUnclaimedRecipe(ItemId.RECIPE_RANDOM_NORMAL, giveGoldOnAllClaimed: false, excludedItemId) : ((randomRatio3.ItemCategoryId != ItemCategoryId.RECIPE_ENCHANTMENT_MASTERY) ? Item.GetRandomLootableItem(PandoraSingleton<GameManager>.Instance.LocalTyche, randomRatio3.ItemCategoryId, randomRatio3.ItemQualityId, randomRatio3.RuneMarkQualityId, warband.WarbandData.AllegianceId, allowedItemIds).Save : warband.GetUnclaimedRecipe(ItemId.RECIPE_RANDOM_MASTERY, giveGoldOnAllClaimed: false, excludedItemId)));
						if (itemSave != null)
						{
							excludedItemId = (ItemId)itemSave.id;
							shop.AddItem(itemSave);
							addedItems.AddItem(itemSave);
						}
					}
				}
			}
		}
		if (PandoraSingleton<HideoutManager>.Exists())
		{
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
		}
	}

	public bool IsRefreshMarket(WeekDayId weekDayId)
	{
		return PandoraSingleton<DataFactory>.Instance.InitData<WeekDayData>((int)weekDayId).RefreshMarket;
	}

	public void RemoveRecipeIOwn(WarbandChest warbandChest)
	{
		List<ItemSave> items = shop.GetItems();
		List<RuneMarkRecipeData> list = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkRecipeData>();
		for (int num = items.Count - 1; num >= 0; num--)
		{
			ItemSave itemSave = items[num];
			ItemId itemId = (ItemId)itemSave.id;
			if (list.Exists((RuneMarkRecipeData x) => x.ItemId == itemId) && warbandChest.HasItem(itemId, ItemQualityId.NORMAL))
			{
				items.RemoveAt(num);
				addedItems.RemoveItem(itemId);
			}
		}
	}
}
