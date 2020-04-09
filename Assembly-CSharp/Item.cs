using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Item
{
	private static StringBuilder descBuilder = new StringBuilder();

	public bool linkedToObjective;

	public Unit owner;

	private ItemData itemData;

	private ItemSave itemSave;

	private ItemRangeData rangeData;

	private string labelName;

	public AnimStyleData StyleData
	{
		get;
		private set;
	}

	public ItemTypeData TypeData
	{
		get;
		private set;
	}

	public ItemSave Save => itemSave;

	public List<Enchantment> Enchantments
	{
		get;
		private set;
	}

	public List<ItemAttributeData> AttributeModifiers
	{
		get;
		private set;
	}

	public bool IsPaired => itemData.Paired;

	public bool IsTwoHanded => TypeData.IsTwoHanded;

	public bool IsLockSlot => itemData.LockSlot;

	public bool IsStackable => itemData.Stackable;

	public bool IsConsumable => ConsumableData != null;

	public bool IsRecipe => RecipeData != null;

	public bool IsUndroppable => itemData.Undroppable;

	public TargetingId TargetingId => itemData.TargetingId;

	public bool TargetAlly => itemData.TargetAlly;

	public BoneId BoneId => itemData.BoneId;

	public int Radius => itemData.Radius;

	public ItemQualityData QualityData
	{
		get;
		private set;
	}

	public ItemQualityJoinItemTypeData QualityTypeData
	{
		get;
		private set;
	}

	public ItemSpeedData SpeedData
	{
		get;
		private set;
	}

	public ItemMutationData MutationData
	{
		get;
		private set;
	}

	public RuneMark RuneMark
	{
		get;
		private set;
	}

	public List<RuneMark> RecipeRuneMarks
	{
		get;
		private set;
	}

	public ItemConsumableData ConsumableData
	{
		get;
		private set;
	}

	public RuneMarkRecipeData RecipeData
	{
		get;
		private set;
	}

	public ItemId Id => itemData.Id;

	public string Name => itemData.Name;

	public string Asset => itemData.Asset;

	public int DamageMin => ((MutationData == null) ? itemData.DamageMin : MutationData.DamageMin) + ((QualityTypeData != null) ? QualityTypeData.DamageMinModifier : 0);

	public int DamageMax => ((MutationData == null) ? itemData.DamageMax : MutationData.DamageMax) + ((QualityTypeData != null) ? QualityTypeData.DamageMaxModifier : 0);

	public int RangeMin => rangeData.MinRange;

	public int RangeMax => rangeData.Range + ((QualityTypeData != null) ? QualityTypeData.RangeModifier : 0);

	public int ArmorAbsorption => itemData.ArmorAbsorption + ((QualityTypeData != null) ? QualityTypeData.ArmorAbsorptionModifier : 0);

	public int Rating => itemData.Rating + ((QualityTypeData != null) ? QualityTypeData.RatingModifier : 0);

	public ProjectileId ProjectileId => itemData.ProjectileId;

	public ProjectileData ProjectileData
	{
		get;
		private set;
	}

	public int Shots => itemData.Shots;

	public string Sound => itemData.Sound;

	public string SoundCat => itemData.SoundCat;

	public List<ItemJoinUnitSlotData> UnitSlots
	{
		get;
		private set;
	}

	public int Amount => itemSave.amount;

	public bool Backup => itemData.Backup;

	public bool IsWyrdStone => Id == ItemId.WYRDSTONE_SHARD || Id == ItemId.WYRDSTONE_FRAGMENT || Id == ItemId.WYRDSTONE_CLUSTER;

	public bool IsIdol => itemData.IsIdol || Id == ItemId.IDOL_MERCENARIES || Id == ItemId.IDOL_POSSESSED || Id == ItemId.IDOL_SISTERS || Id == ItemId.IDOL_SKAVEN;

	public int PriceBuy => itemData.PriceBuy * QualityData.PriceBuyModifier + ((RuneMark != null) ? RuneMark.Cost : 0);

	public int PriceSold => itemData.PriceSold * QualityData.PriceSoldModifier + ((RuneMark != null) ? (RuneMark.Cost / 3) : 0);

	public string LocalizedName
	{
		get
		{
			if (Id == ItemId.GOLD)
			{
				return PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName, itemSave.amount.ToConstantString());
			}
			if (IsTrophy && owner != null)
			{
				return PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName, owner.Name);
			}
			if (IsConsumable)
			{
				return SkillHelper.GetLocalizedName(ConsumableData.SkillId);
			}
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName) + " " + ((RuneMark == null) ? string.Empty : RuneMark.SuffixLocName);
		}
	}

	public string LabelName => labelName;

	public bool IsTrophy
	{
		get;
		private set;
	}

	public Item(ItemSave iSave)
	{
		itemSave = iSave;
		Init((ItemId)itemSave.id, (ItemQualityId)itemSave.qualityId);
	}

	public Item(ItemId id, ItemQualityId qualityId = ItemQualityId.NORMAL)
	{
		itemSave = new ItemSave(id, qualityId);
		Init(id, qualityId);
	}

	private void Init(ItemId id, ItemQualityId qualityId)
	{
		AttributeModifiers = new List<ItemAttributeData>();
		Enchantments = new List<Enchantment>();
		qualityId = ((qualityId == ItemQualityId.NONE) ? ItemQualityId.NORMAL : qualityId);
		itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)id);
		labelName = "item_name_" + Name;
		TypeData = PandoraSingleton<DataFactory>.Instance.InitData<ItemTypeData>((int)itemData.ItemTypeId);
		if (TypeData.Id == ItemTypeId.RECIPE_ENCHANTMENT_NORMAL || TypeData.Id == ItemTypeId.RECIPE_ENCHANTMENT_MASTERY)
		{
			qualityId = ItemQualityId.NORMAL;
		}
		QualityData = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityData>((int)qualityId);
		if (QualityData.Id != ItemQualityId.NORMAL)
		{
			List<ItemQualityJoinItemTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityJoinItemTypeData>(new string[2]
			{
				"fk_item_quality_id",
				"fk_item_type_id"
			}, new string[2]
			{
				((int)qualityId).ToConstantString(),
				((int)itemData.ItemTypeId).ToConstantString()
			});
			QualityTypeData = list[0];
		}
		StyleData = PandoraSingleton<DataFactory>.Instance.InitData<AnimStyleData>((int)itemData.AnimStyleId);
		rangeData = PandoraSingleton<DataFactory>.Instance.InitData<ItemRangeData>((int)itemData.ItemRangeId);
		SpeedData = PandoraSingleton<DataFactory>.Instance.InitData<ItemSpeedData>((int)itemData.ItemSpeedId);
		UnitSlots = PandoraSingleton<DataFactory>.Instance.InitData<ItemJoinUnitSlotData>("fk_item_id", itemData.Id.ToIntString());
		List<ItemConsumableData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemConsumableData>(new string[2]
		{
			"fk_item_id",
			"fk_item_quality_id"
		}, new string[2]
		{
			((int)id).ToConstantString(),
			((int)qualityId).ToConstantString()
		});
		if (list2 != null && list2.Count > 0)
		{
			ConsumableData = list2[0];
		}
		if (itemData.ProjectileId != 0)
		{
			ProjectileData = PandoraSingleton<DataFactory>.Instance.InitData<ProjectileData>((int)ProjectileId);
		}
		List<RuneMarkRecipeData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkRecipeData>("fk_item_id", ((int)id).ToConstantString());
		if (list3 != null && list3.Count > 0)
		{
			RecipeData = list3[0];
			RecipeRuneMarks = new List<RuneMark>();
			for (int i = 0; i < list3.Count; i++)
			{
				RecipeRuneMarks.Add(new RuneMark(list3[i].RuneMarkId, list3[i].RuneMarkQualityId, AllegianceId.ORDER, ItemTypeId.NONE));
			}
		}
		SetModifiers(MutationId.NONE);
		if (itemSave.runeMarkId != 0)
		{
			RuneMark = new RuneMark((RuneMarkId)itemSave.runeMarkId, (RuneMarkQualityId)itemSave.runeMarkQualityId, (AllegianceId)itemSave.allegianceId, TypeData.Id);
		}
		if (Id == ItemId.BOUNTY_HUMAN_MERCS || Id == ItemId.BOUNTY_HUMAN_SISTERS || Id == ItemId.BOUNTY_OGRE_MERC || Id == ItemId.BOUNTY_POSSESSED || Id == ItemId.BOUNTY_SKAVEN || Id == ItemId.BOUNTY_HUMAN_WITCH_HUNTERS || Id == ItemId.BOUNTY_UNDEAD)
		{
			IsTrophy = true;
		}
	}

	public void SetModifiers(MutationId mutationId)
	{
		Enchantments.Clear();
		MutationData = null;
		List<ItemEnchantmentData> list = new List<ItemEnchantmentData>();
		if (itemData.MutationBased)
		{
			List<ItemMutationData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemMutationData>(new string[2]
			{
				"fk_item_id",
				"fk_mutation_id"
			}, new string[2]
			{
				((int)Id).ToConstantString(),
				((int)mutationId).ToConstantString()
			});
			if (list2.Count > 0)
			{
				MutationData = list2[0];
				SpeedData = PandoraSingleton<DataFactory>.Instance.InitData<ItemSpeedData>((int)MutationData.ItemSpeedId);
			}
			AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<ItemAttributeData>(new string[3]
			{
				"fk_item_id",
				"fk_item_quality_id",
				"fk_mutation_id"
			}, new string[3]
			{
				((int)Id).ToConstantString(),
				((int)QualityData.Id).ToConstantString(),
				((int)mutationId).ToConstantString()
			});
			list = PandoraSingleton<DataFactory>.Instance.InitData<ItemEnchantmentData>(new string[3]
			{
				"fk_item_id",
				"fk_item_quality_id",
				"fk_mutation_id"
			}, new string[3]
			{
				((int)Id).ToConstantString(),
				((int)QualityData.Id).ToConstantString(),
				((int)mutationId).ToConstantString()
			});
		}
		else
		{
			AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<ItemAttributeData>(new string[2]
			{
				"fk_item_id",
				"fk_item_quality_id"
			}, new string[2]
			{
				((int)Id).ToConstantString(),
				((int)QualityData.Id).ToConstantString()
			});
			list = PandoraSingleton<DataFactory>.Instance.InitData<ItemEnchantmentData>(new string[2]
			{
				"fk_item_id",
				"fk_item_quality_id"
			}, new string[2]
			{
				((int)Id).ToConstantString(),
				((int)QualityData.Id).ToConstantString()
			});
		}
		for (int i = 0; i < list.Count; i++)
		{
			Enchantments.Add(new Enchantment(list[i].EnchantmentId, null, null, orig: false, innate: true));
		}
	}

	public bool AddRuneMark(RuneMarkId runeMarkId, RuneMarkQualityId runeQualityId, AllegianceId allegianceId)
	{
		if (runeQualityId <= QualityData.RuneMarkQualityIdMax && (RuneMark == null || RuneMark.Data.Id == RuneMarkId.NONE))
		{
			itemSave.runeMarkId = (int)runeMarkId;
			itemSave.runeMarkQualityId = (int)runeQualityId;
			itemSave.allegianceId = (int)allegianceId;
			RuneMark = new RuneMark(runeMarkId, runeQualityId, allegianceId, TypeData.Id);
			return true;
		}
		return false;
	}

	public bool CanAddRuneMark()
	{
		return QualityData.Id > ItemQualityId.NORMAL && (RuneMark == null || RuneMark.Data.Id == RuneMarkId.NONE);
	}

	public bool HasEnchantment(EnchantmentId enchantmentId)
	{
		bool result = false;
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Id == enchantmentId)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool UpdateEnchantmentsDuration(Unit currentUnit)
	{
		bool result = false;
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].UpdateDuration(currentUnit))
			{
				Enchantments.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	public bool UpdateEnchantments(UnitStateId unitStateId)
	{
		bool result = false;
		for (int num = Enchantments.Count - 1; num >= 0; num--)
		{
			if (Enchantments[num].UpdateStatus(unitStateId))
			{
				Enchantments.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	public int GetRating()
	{
		int num = 0;
		if (itemData != null)
		{
			num += Rating;
			if (RuneMark != null)
			{
				num += RuneMark.QualityItemTypeData.Rating;
			}
		}
		return num;
	}

	public Sprite GetIcon()
	{
		return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item/" + itemData.Asset.ToLowerInvariant(), cached: true);
	}

	public Sprite GetRuneIcon()
	{
		if (RuneMark != null)
		{
			return RuneMark.GetIcon();
		}
		if (!IsConsumable && QualityData.RuneMarkQualityIdMax != 0)
		{
			return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("runemark/" + QualityData.RuneMarkQualityIdMax.ToLowerString() + "_empty", cached: true);
		}
		return null;
	}

	public ItemAssetData GetAssetData(RaceId raceId, WarbandId warbandId, UnitId unitId)
	{
		ItemAssetData result = null;
		List<ItemAssetData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemAssetData>(new string[3]
		{
			"fk_item_id",
			"fk_race_id",
			"fk_warband_id"
		}, new string[3]
		{
			((int)Id).ToConstantString(),
			((int)raceId).ToConstantString(),
			((int)warbandId).ToConstantString()
		});
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].UnitId == unitId)
			{
				return list[i];
			}
			if (list[i].UnitId == UnitId.NONE)
			{
				result = list[i];
			}
		}
		return result;
	}

	public string GetRuneNameDesc()
	{
		if (RuneMark != null)
		{
			return RuneMark.FullLocName + " :\n" + RuneMark.LocShort;
		}
		if (QualityData.RuneMarkQualityIdMax != 0)
		{
			AllegianceId allegianceId = (AllegianceId)Save.allegianceId;
			if (owner == null && PandoraSingleton<HideoutManager>.Exists())
			{
				UnitMenuController currentUnit = PandoraSingleton<HideoutManager>.Instance.currentUnit;
				if (currentUnit != null)
				{
					allegianceId = currentUnit.unit.AllegianceId;
				}
			}
			else if (owner == null && PandoraSingleton<MissionManager>.Exists())
			{
				UnitController currentUnit2 = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
				if (currentUnit2 != null)
				{
					allegianceId = currentUnit2.unit.AllegianceId;
				}
			}
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById((allegianceId != AllegianceId.ORDER) ? "item_enchant_type_mark_empty" : "item_enchant_type_rune_empty");
		}
		return string.Empty;
	}

	private bool IsDualWield()
	{
		for (int i = 0; i < Enchantments.Count; i++)
		{
			if (Enchantments[i].Id == EnchantmentId.ITEM_DUAL_WIELD)
			{
				return true;
			}
		}
		return false;
	}

	public string GetLocalizedDescription(UnitSlotId? equippedSlot)
	{
		if (IsConsumable)
		{
			return SkillHelper.GetLocalizedDescription(ConsumableData.SkillId);
		}
		if (IsRecipe)
		{
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_desc_recipe_" + RecipeRuneMarks[0].Name + "_normal") + "\n" + RecipeRuneMarks[0].LocDesc;
		}
		string text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_desc_" + Name + "_" + QualityData.Name);
		if (IsDualWield())
		{
			text = text + "\n" + PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_dualwield_desc");
		}
		return text;
	}

	public static string GetLocalizedName(ItemId id)
	{
		return PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_name_" + id.ToLowerString());
	}

	public static string GetLocalizedName(ItemId id, ItemQualityId qualityId)
	{
		List<ItemConsumableData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemConsumableData>(new string[2]
		{
			"fk_item_id",
			"fk_item_quality_id"
		}, new string[2]
		{
			((int)id).ToConstantString(),
			((int)qualityId).ToConstantString()
		});
		if (list != null && list.Count > 0)
		{
			return SkillHelper.GetLocalizedName(list[0].SkillId);
		}
		return GetLocalizedName(id);
	}

	public static Item GetItemReward(List<SearchRewardItemData> rewardItems, Tyche tyche)
	{
		SearchRewardItemData randomRatio = SearchRewardItemData.GetRandomRatio(rewardItems, tyche);
		SearchRewardData searchRewardData = PandoraSingleton<DataFactory>.Instance.InitData<SearchRewardData>((int)randomRatio.SearchRewardId);
		List<AllegianceId> list = new List<AllegianceId>();
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
		{
			list.Add(PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].WarData.AllegianceId);
		}
		int index = tyche.Rand(0, list.Count);
		return GetRandomLootableItem(tyche, randomRatio.ItemCategoryId, searchRewardData.ItemQualityId, searchRewardData.RuneMarkQualityId, list[index]);
	}

	public static Item GetRandomLootableItem(Tyche tyche, ItemCategoryId categoryId, ItemQualityId qualityId, RuneMarkQualityId runeMarkQualityId, AllegianceId allegianceId, List<ItemId> allowedWeaponsArmors = null, List<ItemId> additionalLootableItemsId = null, List<ItemId> exludeItemIds = null)
	{
		List<ItemTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemTypeData>("fk_item_category_id", ((int)categoryId).ToConstantString());
		List<ItemTypeId> types = list.ConvertAll((ItemTypeData x) => x.Id);
		return GetRandomLootableItem(tyche, types, qualityId, runeMarkQualityId, allegianceId, allowedWeaponsArmors, additionalLootableItemsId, exludeItemIds);
	}

	public static Item GetRandomLootableItem(Tyche tyche, List<ItemTypeId> types, ItemQualityId qualityId, RuneMarkQualityId runeMarkQualityId, AllegianceId allegianceId, List<ItemId> allowedWeaponsArmors = null, List<ItemId> additionalLootableItemsId = null, List<ItemId> exludeItemIds = null)
	{
		if (types.Count == 1)
		{
			if (types[0] == ItemTypeId.RECIPE_ENCHANTMENT_NORMAL)
			{
				return new Item(ItemId.RECIPE_RANDOM_NORMAL);
			}
			if (types[0] == ItemTypeId.RECIPE_ENCHANTMENT_MASTERY)
			{
				return new Item(ItemId.RECIPE_RANDOM_MASTERY);
			}
		}
		List<ItemData> list = new List<ItemData>();
		for (int i = 0; i < types.Count; i++)
		{
			list.AddRange(PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(new string[2]
			{
				"fk_item_type_id",
				"lootable"
			}, new string[2]
			{
				((int)types[i]).ToConstantString(),
				"1"
			}));
		}
		if (additionalLootableItemsId != null)
		{
			for (int j = 0; j < additionalLootableItemsId.Count; j++)
			{
				ItemId itemId = additionalLootableItemsId[j];
				if (!list.Exists((ItemData x) => x.Id == itemId))
				{
					list.Add(PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)additionalLootableItemsId[j]));
				}
			}
		}
		if (allowedWeaponsArmors != null)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				ItemTypeData itemTypeData = PandoraSingleton<DataFactory>.Instance.InitData<ItemTypeData>((int)list[num].ItemTypeId);
				ItemCategoryId itemCategoryId = itemTypeData.ItemCategoryId;
				if ((itemCategoryId == ItemCategoryId.WEAPONS || itemCategoryId == ItemCategoryId.ARMOR) && allowedWeaponsArmors.IndexOf(list[num].Id, ItemIdComparer.Instance) == -1)
				{
					list.RemoveAt(num);
				}
			}
		}
		int index = tyche.Rand(0, list.Count);
		qualityId = ((list[index].ItemTypeId == ItemTypeId.RECIPE_ENCHANTMENT_NORMAL || list[index].ItemTypeId == ItemTypeId.RECIPE_ENCHANTMENT_MASTERY) ? ItemQualityId.NORMAL : GetClosestQuality(list[index].Id, qualityId));
		Item item = new Item(new ItemSave(list[index].Id, qualityId));
		if (runeMarkQualityId > RuneMarkQualityId.NONE && item.CanAddRuneMark())
		{
			RuneMarkId randomRuneMark = GetRandomRuneMark(tyche, item, allegianceId);
			if (randomRuneMark != 0)
			{
				item.AddRuneMark(randomRuneMark, runeMarkQualityId, allegianceId);
			}
		}
		return item;
	}

	public static RuneMarkId GetRandomRuneMark(Tyche tyche, Item item, AllegianceId allegianceId)
	{
		List<RuneMarkJoinItemTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkJoinItemTypeData>("fk_item_type_id", ((int)item.TypeData.Id).ToConstantString());
		List<RuneMarkId> list2 = new List<RuneMarkId>();
		for (int i = 0; i < list.Count; i++)
		{
			RuneMarkData runeMarkData = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkData>((int)list[i].RuneMarkId);
			if ((allegianceId == AllegianceId.ORDER && runeMarkData.Rune) || (allegianceId == AllegianceId.DESTRUCTION && runeMarkData.Mark))
			{
				list2.Add(runeMarkData.Id);
			}
		}
		if (list2.Count > 0)
		{
			int index = tyche.Rand(0, list2.Count);
			return list2[index];
		}
		return RuneMarkId.NONE;
	}

	public static ItemQualityId GetClosestQuality(ItemId itemId, ItemQualityId desiredQualityId)
	{
		ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)itemId);
		return GetClosestQuality(itemData.ItemTypeId, desiredQualityId);
	}

	public static ItemQualityId GetClosestQuality(ItemTypeId itemTypeId, ItemQualityId desiredQualityId)
	{
		List<ItemQualityId> list = new List<ItemQualityId>();
		list.Add(ItemQualityId.NORMAL);
		List<ItemQualityJoinItemTypeData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityJoinItemTypeData>("fk_item_type_id", ((int)itemTypeId).ToConstantString());
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i].ItemQualityId);
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == desiredQualityId)
			{
				return desiredQualityId;
			}
		}
		ItemQualityId itemQualityId = ItemQualityId.NONE;
		for (int k = 0; k < list.Count; k++)
		{
			if (itemQualityId == ItemQualityId.NONE || Mathf.Abs(list[k] - desiredQualityId) < Mathf.Abs(itemQualityId - desiredQualityId))
			{
				itemQualityId = list[k];
			}
		}
		return itemQualityId;
	}

	public static void SortEmptyItems(List<Item> items, int startIdx)
	{
		for (int i = startIdx; i < items.Count - 1; i++)
		{
			if (items[i].Id == ItemId.NONE && items[i + 1].Id != 0)
			{
				Item value = items[i];
				items[i] = items[i + 1];
				items[i + 1] = value;
				i = startIdx - 1;
			}
		}
	}

	public bool IsSame(Item item)
	{
		if (Id == item.Id && QualityData.Id == item.QualityData.Id && ((RuneMark == null && item.RuneMark == null) || (RuneMark != null && item.RuneMark != null && RuneMark.Data.Id == item.RuneMark.Data.Id && RuneMark.QualityData.Id == item.RuneMark.QualityData.Id)))
		{
			return true;
		}
		return false;
	}

	public bool IsSold()
	{
		List<ItemSave> items = PandoraSingleton<HideoutManager>.Instance.Market.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].id == itemSave.id && items[i].qualityId == itemSave.qualityId && items[i].runeMarkId == itemSave.runeMarkId && items[i].runeMarkQualityId == itemSave.runeMarkQualityId)
			{
				return true;
			}
		}
		return false;
	}
}
