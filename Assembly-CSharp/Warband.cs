using System;
using System.Collections.Generic;
using UnityEngine;

public class Warband
{
    private static readonly UnitTypeId[] OutsidersTypeIds = new UnitTypeId[10]
    {
        UnitTypeId.HENCHMEN,
        UnitTypeId.HERO_1,
        UnitTypeId.LEADER,
        UnitTypeId.IMPRESSIVE,
        UnitTypeId.HENCHMEN,
        UnitTypeId.HERO_1,
        UnitTypeId.HERO_1,
        UnitTypeId.LEADER,
        UnitTypeId.HENCHMEN,
        UnitTypeId.HERO_1
    };

    private readonly List<WarbandAttributeData> attributeDataList;

    private readonly int[] attributes;

    public bool rankGained;

    private readonly WarbandSave warbandSave;

    private int maxRank = -1;

    private List<ItemId> allowedItemIds = new List<ItemId>();

    private List<ItemId> addtionnalMarketItems = new List<ItemId>();

    private List<ItemId> unclaimedRecipe = new List<ItemId>();

    private readonly List<int> tempList = new List<int>(1)
    {
        -1
    };

    public List<UnitId> HireableUnitIds
    {
        get;
        private set;
    }

    public List<UnitData> HireableOutsiderUnitIds
    {
        get;
        private set;
    }

    public List<ItemId> AddtionnalMarketItems
    {
        get;
        private set;
    }

    public int[] HireableUnitTypeRank
    {
        get;
        private set;
    }

    public List<WarbandSkill> skills
    {
        get;
        private set;
    }

    public List<WarbandEnchantment> enchantments
    {
        get;
        private set;
    }

    public WarbandId Id
    {
        get;
        private set;
    }

    public WarbandData WarbandData
    {
        get;
        private set;
    }

    public List<Unit> Units
    {
        get;
        private set;
    }

    public List<Unit> Outsiders
    {
        get;
        private set;
    }

    public List<Faction> Factions
    {
        get;
        private set;
    }

    public Dictionary<int, int>[] WyrdstoneDensityModifiers
    {
        get;
        private set;
    }

    public Dictionary<int, int>[] SearchDensityModifiers
    {
        get;
        private set;
    }

    public Dictionary<MarketEventId, int> MarketEventModifiers
    {
        get;
        private set;
    }

    public Dictionary<ProcMissionRatingId, int> MissionRatingModifiers
    {
        get;
        set;
    }

    public EventLogger Logger
    {
        get;
        private set;
    }

    public int Rank
    {
        get
        {
            return GetAttribute(WarbandAttributeId.RANK);
        }
        set
        {
            warbandSave.rank = value;
            SetAttribute(WarbandAttributeId.RANK, value);
        }
    }

    public int Xp
    {
        get
        {
            return warbandSave.xp;
        }
        set
        {
            warbandSave.xp = value;
        }
    }

    public Warband(WarbandSave ws)
    {
        warbandSave = ws;
        ValidateOutsiders();
        Logger = new EventLogger(ws.stats.history);
        MarketEventModifiers = new Dictionary<MarketEventId, int>(MarketEventIdComparer.Instance);
        MissionRatingModifiers = new Dictionary<ProcMissionRatingId, int>(ProcMissionRatingIdComparer.Instance);
        AddtionnalMarketItems = new List<ItemId>();
        unclaimedRecipe = new List<ItemId>();
        WyrdstoneDensityModifiers = new Dictionary<int, int>[5];
        SearchDensityModifiers = new Dictionary<int, int>[5];
        for (int i = 0; i < 5; i++)
        {
            SearchDensityModifiers[i] = new Dictionary<int, int>();
            WyrdstoneDensityModifiers[i] = new Dictionary<int, int>();
        }
        attributeDataList = PandoraSingleton<DataFactory>.Instance.InitData<WarbandAttributeData>();
        Id = (WarbandId)warbandSave.id;
        WarbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)Id);
        attributes = new int[63];
        Array.Copy(warbandSave.stats.stats, attributes, 63);
        Units = new List<Unit>();
        for (int j = 0; j < warbandSave.units.Count; j++)
        {
            Unit item = new Unit(warbandSave.units[j]);
            Units.Add(item);
        }
        Outsiders = new List<Unit>();
        for (int k = 0; k < warbandSave.outsiders.Count; k++)
        {
            warbandSave.outsiders[k].attributes.Clear();
            warbandSave.outsiders[k].activeSkills.Clear();
            warbandSave.outsiders[k].passiveSkills.Clear();
            warbandSave.outsiders[k].spells.Clear();
            Unit item2 = new Unit(warbandSave.outsiders[k]);
            Outsiders.Add(item2);
        }
        HireableUnitIds = new List<UnitId>();
        HireableOutsiderUnitIds = new List<UnitData>();
        HireableUnitTypeRank = new int[10];
        skills = new List<WarbandSkill>();
        for (int l = 0; l < warbandSave.skills.Count; l++)
        {
            AddSkill(warbandSave.skills[l], isNew: false, updateAttributes: false);
        }
        Factions = new List<Faction>();
        for (int m = 0; m < warbandSave.factions.Count; m++)
        {
            Factions.Add(new Faction(warbandSave, warbandSave.factions[m]));
            for (int n = 0; n < Factions[m].Rewards.Count; n++)
            {
                if (Factions[m].HasRank((int)Factions[m].Rewards[n].FactionRankId))
                {
                    AddSkill(Factions[m].Rewards[n].WarbandSkillId);
                }
            }
        }
        RefreshPlayerSkills();
        UpdateAttributes();
    }

    public bool ValidateWarbandForInvite(bool inMission)
    {
        WarbandSave warbandSave = GetWarbandSave();
        if ((!inMission && warbandSave.inMission) || PandoraSingleton<Hephaestus>.Instance.joiningLobby == null)
        {
            return false;
        }
        string reason;
        if (PandoraSingleton<Hephaestus>.Instance.joiningLobby.isExhibition)
        {
            if (!PandoraUtils.IsBetween(GetSkirmishRating(), PandoraSingleton<Hephaestus>.Instance.joiningLobby.ratingMin, PandoraSingleton<Hephaestus>.Instance.joiningLobby.ratingMax) || !IsSkirmishAvailable(out reason))
            {
                return false;
            }
            return true;
        }
        if (!PandoraUtils.IsBetween(GetRating(), PandoraSingleton<Hephaestus>.Instance.joiningLobby.ratingMin, PandoraSingleton<Hephaestus>.Instance.joiningLobby.ratingMax) || !IsContestAvailable(out reason))
        {
            return false;
        }
        return true;
    }

    private void ValidateOutsiders()
    {
        if (warbandSave == null || warbandSave.outsiders == null)
        {
            return;
        }
        for (int num = warbandSave.outsiders.Count - 1; num >= 0; num--)
        {
            WarbandId warbandId = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(warbandSave.outsiders[num].stats.id).WarbandId;
            if (!PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)warbandId).Basic)
            {
                warbandSave.outsiders.RemoveAt(num);
            }
        }
    }

    public void UpdateAttributes()
    {
        for (int i = 0; i < attributeDataList.Count; i++)
        {
            if (!attributeDataList[i].Persistent)
            {
                SetAttribute(attributeDataList[i].Id, attributeDataList[i].BaseValue);
            }
        }
        SetAttribute(WarbandAttributeId.RANK, warbandSave.rank);
        for (int j = 0; j < HireableUnitTypeRank.Length; j++)
        {
            HireableUnitTypeRank[j] = 1;
        }
        int num = 5;
        for (int k = 0; k < num; k++)
        {
            SearchDensityModifiers[k].Clear();
            WyrdstoneDensityModifiers[k].Clear();
        }
        MissionRatingModifiers.Clear();
        MarketEventModifiers.Clear();
        HireableUnitIds.Clear();
        HireableOutsiderUnitIds.Clear();
        AddtionnalMarketItems.Clear();
        if (warbandSave.currentDate == Constant.GetInt(ConstantId.CAL_DAY_START))
        {
            for (ProcMissionRatingId procMissionRatingId = ProcMissionRatingId.NONE; procMissionRatingId < ProcMissionRatingId.MAX_VALUE; procMissionRatingId++)
            {
                if (procMissionRatingId != ProcMissionRatingId.NORMAL)
                {
                    MissionRatingModifiers.Add(procMissionRatingId, -100);
                }
            }
        }
        List<UnitData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>("fk_warband_id", ((int)Id).ToString(), "base", "1");
        for (int l = 0; l < list.Count; l++)
        {
            if (IsUnitTypeUnlocked(list[l].UnitTypeId))
            {
                HireableUnitIds.Add(list[l].Id);
                HireableOutsiderUnitIds.Add(list[l]);
            }
        }
        for (int m = 0; m < skills.Count; m++)
        {
            ApplySkill(skills[m]);
        }
        ApplySkill(new WarbandSkill(PandoraSingleton<DataFactory>.Instance.InitData<MarketEventData>(warbandSave.marketEventId).WarbandSkillId));
        UpdateFactionAttributes();
    }

    public bool HasUnclaimedRecipe(ItemId randomRecipe)
    {
        unclaimedRecipe.Clear();
        ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)randomRecipe);
        List<ItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(new string[2]
        {
            "fk_item_type_id",
            "lootable"
        }, new string[2]
        {
            itemData.ItemTypeId.ToIntString(),
            "1"
        });
        for (int i = 0; i < list.Count; i++)
        {
            if (!PandoraSingleton<HideoutManager>.Instance.WarbandChest.HasItem(list[i].Id, ItemQualityId.NORMAL) && !PandoraSingleton<HideoutManager>.Instance.Market.HasItem(list[i].Id, ItemQualityId.NORMAL))
            {
                unclaimedRecipe.Add(list[i].Id);
            }
        }
        return unclaimedRecipe.Count > 0;
    }

    public ItemSave GetUnclaimedRecipe(ItemId randomRecipe, bool giveGoldOnAllClaimed = true, ItemId excludedItemId = ItemId.NONE)
    {
        unclaimedRecipe.Clear();
        ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)randomRecipe);
        List<ItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(new string[2]
        {
            "fk_item_type_id",
            "lootable"
        }, new string[2]
        {
            itemData.ItemTypeId.ToIntString(),
            "1"
        });
        for (int i = 0; i < list.Count; i++)
        {
            if (!PandoraSingleton<HideoutManager>.Instance.WarbandChest.HasItem(list[i].Id, ItemQualityId.NORMAL) && !PandoraSingleton<HideoutManager>.Instance.Market.HasItem(list[i].Id, ItemQualityId.NORMAL) && list[i].Id != excludedItemId)
            {
                unclaimedRecipe.Add(list[i].Id);
            }
        }
        if (unclaimedRecipe.Count > 0)
        {
            return new ItemSave(unclaimedRecipe[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, unclaimedRecipe.Count)]);
        }
        if (giveGoldOnAllClaimed)
        {
            ItemSave itemSave = new ItemSave(ItemId.GOLD);
            itemSave.amount = itemData.PriceSold;
            return itemSave;
        }
        return null;
    }

    private void UpdateFactionAttributes()
    {
        AddToAttribute(WarbandAttributeId.REQUEST_TIME_MODIFIER, warbandSave.nextShipmentExtraDays);
        int num = 0;
        while (true)
        {
            if (num < Factions.Count)
            {
                if (Factions[num].Primary && warbandSave.lastShipmentFailed)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        FactionConsequenceData factionConsequenceData = PandoraSingleton<DataFactory>.Instance.InitData<FactionConsequenceData>(new string[2]
        {
            "fk_faction_id",
            "late_shipment_count"
        }, new string[2]
        {
            ((int)Factions[num].Data.Id).ToString(),
            warbandSave.lateShipmentCount.ToString()
        })[0];
        AddToAttribute(WarbandAttributeId.REQUEST_PRICE_GLOBAL_PERC, factionConsequenceData.NextShipmentGoldRewardModifierPerc);
        AddToAttribute(WarbandAttributeId.REQUEST_WEIGHT_PERC, factionConsequenceData.NextShipmentRequestModifierPerc);
    }

    public void CheckRespecPoints()
    {
        if (warbandSave.availaibleRespec == -1)
        {
            warbandSave.availaibleRespec = 1;
            int @int = Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR);
            int num = (int)((float)warbandSave.currentDate / (float)@int);
            warbandSave.availaibleRespec += num;
            Logger.AddHistory(@int * (num + 1), EventLogger.LogEvent.RESPEC_POINT, 0);
        }
    }

    public void ResetPlayerSkills()
    {
        warbandSave.availaibleRespec = Mathf.Max(0, warbandSave.availaibleRespec - 1);
        for (int num = skills.Count - 1; num >= 0; num--)
        {
            if (skills[num].TypeId == WarbandSkillTypeId.PLAYER_SKILL)
            {
                warbandSave.skills.Remove(skills[num].Id);
                skills.RemoveAt(num);
            }
        }
    }

    public List<WarbandSkill> GetPlayerSkills()
    {
        List<WarbandSkill> list = new List<WarbandSkill>();
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].TypeId == WarbandSkillTypeId.PLAYER_SKILL)
            {
                list.Add(skills[i]);
            }
        }
        return list;
    }

    public int GetPlayerSkillsAvailablePoints()
    {
        int num = GetAttribute(WarbandAttributeId.PLAYER_SKILL_POINTS);
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].TypeId == WarbandSkillTypeId.PLAYER_SKILL)
            {
                num -= skills[i].Data.Points;
                if (skills[i].Data.WarbandSkillIdPrerequisite != 0)
                {
                    WarbandSkillData warbandSkillData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillData>((int)skills[i].Data.WarbandSkillIdPrerequisite);
                    num -= warbandSkillData.Points;
                }
            }
        }
        return num;
    }

    public void RefreshPlayerSkills(bool isNew = false)
    {
        int attribute = GetAttribute(WarbandAttributeId.OUTSIDERS_COUNT);
        for (int i = 0; i <= PandoraSingleton<GameManager>.Instance.Profile.Rank; i++)
        {
            PlayerRankData playerRankData = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>(i);
            if (playerRankData != null)
            {
                AddSkill(playerRankData.WarbandSkillId, isNew);
            }
        }
        if (isNew && attribute == 0 && GetAttribute(WarbandAttributeId.OUTSIDERS_COUNT) > 0)
        {
            AddOutsiderRotationEvent();
        }
    }

    public void AddOutsiderRotationEvent()
    {
        List<WeekDayData> list = PandoraSingleton<DataFactory>.Instance.InitData<WeekDayData>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].RefreshOutsiders)
            {
                int nextDay = PandoraSingleton<HideoutManager>.Instance.Date.GetNextDay(list[i].Id);
                if (!Logger.HasEventAtDay(EventLogger.LogEvent.OUTSIDER_ROTATION, nextDay))
                {
                    Logger.AddHistory(nextDay, EventLogger.LogEvent.OUTSIDER_ROTATION, 0);
                }
            }
        }
    }

    public bool HasSkill(WarbandSkillId skillId, bool includeMastery)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].Id == skillId || (includeMastery && skills[i].Data.WarbandSkillIdPrerequisite == skillId))
            {
                return true;
            }
        }
        return false;
    }

    public void LearnSkill(WarbandSkill skill)
    {
        warbandSave.skills.Add(skill.Id);
        AddSkill(skill.Id, isNew: true);
    }

    public void AddSkill(WarbandSkillId skillId, bool isNew = false, bool updateAttributes = true)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].Id == skillId)
            {
                return;
            }
        }
        bool flag = true;
        WarbandSkill warbandSkill = new WarbandSkill(skillId);
        if (warbandSkill.IsMastery)
        {
            for (int num = skills.Count - 1; num >= 0; num--)
            {
                if (skills[num].Id == warbandSkill.Data.WarbandSkillIdPrerequisite)
                {
                    skills[num] = warbandSkill;
                    flag = false;
                    break;
                }
            }
        }
        else
        {
            for (int j = 0; j < skills.Count; j++)
            {
                if (skills[j].Data.WarbandSkillIdPrerequisite == warbandSkill.Id)
                {
                    return;
                }
            }
        }
        if (flag)
        {
            skills.Add(warbandSkill);
        }
        if (updateAttributes)
        {
            UpdateAttributes();
        }
        if (!isNew)
        {
            return;
        }
        List<WarbandSkillItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillItemData>("fk_warband_skill_id", warbandSkill.Id.ToIntString());
        for (int k = 0; k < list.Count; k++)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(list[k].ItemId, list[k].ItemQualityId, list[k].RuneMarkId, list[k].RuneMarkQualityId, AllegianceId.NONE, list[k].Quantity);
        }
        List<WarbandSkillFreeOutsiderData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillFreeOutsiderData>("fk_warband_skill_id", warbandSkill.Id.ToIntString());
        for (int l = 0; l < list2.Count; l++)
        {
            int num2 = list2[l].Rank;
            ItemQualityId itemQualityId = ItemQualityId.NONE;
            if (num2 == -1)
            {
                for (int m = 0; m < Units.Count; m++)
                {
                    num2 = Mathf.Max(num2, Units[m].Rank);
                }
                num2 = Mathf.Clamp(num2, 0, 7);
                itemQualityId = ((num2 < 6) ? ItemQualityId.GOOD : ItemQualityId.BEST);
                itemQualityId = ((num2 <= 2) ? ItemQualityId.NORMAL : itemQualityId);
            }
            Unit unit = Unit.GenerateUnit(list2[l].UnitId, num2);
            unit.UnitSave.isFreeOutsider = true;
            unit.UnitSave.isOutsider = true;
            List<WarbandSkillFreeOutsiderItemData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillFreeOutsiderItemData>("fk_warband_skill_free_outsider_id", list2[l].Id.ToIntString());
            for (int n = 0; n < list3.Count; n++)
            {
                unit.EquipItem(list3[n].UnitSlotId, new ItemSave(list3[n].ItemId, (itemQualityId != 0) ? itemQualityId : list3[n].ItemQualityId, list3[n].RuneMarkId, list3[n].RuneMarkQualityId));
            }
            warbandSave.outsiders.Add(unit.UnitSave);
            Outsiders.Add(unit);
        }
    }

    private void ApplySkill(WarbandSkill skill)
    {
        ApplyEnchantments(skill.Enchantments);
        for (int i = 0; i < skill.HireableUnits.Count; i++)
        {
            WarbandSkillUnitData warbandSkillUnitData = skill.HireableUnits[i];
            if (warbandSkillUnitData.UnitId != 0)
            {
                if (warbandSkillUnitData.BaseUnit && !HireableUnitIds.Contains(warbandSkillUnitData.UnitId, UnitIdComparer.Instance))
                {
                    HireableUnitIds.Add(warbandSkillUnitData.UnitId);
                }
                if (!HireableOutsiderUnitIds.Exists((UnitData x) => x.Id == warbandSkillUnitData.UnitId))
                {
                    HireableOutsiderUnitIds.Add(PandoraSingleton<DataFactory>.Instance.InitData<UnitData>((int)warbandSkillUnitData.UnitId));
                }
            }
        }
        for (int j = 0; j < skill.UnitTypeRankDatas.Count; j++)
        {
            HireableUnitTypeRank[(int)skill.UnitTypeRankDatas[j].UnitTypeId] = Mathf.Max(skill.UnitTypeRankDatas[j].Rank, HireableUnitTypeRank[(int)skill.UnitTypeRankDatas[j].UnitTypeId]);
        }
        for (int k = 0; k < skill.MarketItems.Count; k++)
        {
            if (!AddtionnalMarketItems.Contains(skill.MarketItems[k].ItemId, ItemIdComparer.Instance))
            {
                AddtionnalMarketItems.Add(skill.MarketItems[k].ItemId);
            }
        }
    }

    public void GenerateContactItems()
    {
        List<WarbandContactItemQualityData> datas = PandoraSingleton<DataFactory>.Instance.InitData<WarbandContactItemQualityData>("fk_warband_rank_id", Rank.ToString());
        List<ItemTypeId> list = new List<ItemTypeId>();
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].ContactsData.Count <= 0)
            {
                continue;
            }
            WarbandSkillWarbandContactData randomRatio = WarbandSkillWarbandContactData.GetRandomRatio(skills[i].ContactsData, PandoraSingleton<GameManager>.Instance.LocalTyche);
            if (randomRatio.WarbandContactId != 0)
            {
                list.Clear();
                WarbandContactData warbandContactData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandContactData>((int)randomRatio.WarbandContactId);
                WarbandContactItemQualityData randomRatio2 = WarbandContactItemQualityData.GetRandomRatio(datas, PandoraSingleton<GameManager>.Instance.LocalTyche);
                ItemQualityId itemQualityId = randomRatio2.ItemQualityId;
                list.Add(warbandContactData.ItemTypeId);
                Item item = Item.GetRandomLootableItem(PandoraSingleton<GameManager>.Instance.LocalTyche, list, itemQualityId, RuneMarkQualityId.NONE, WarbandData.AllegianceId);
                if (item.Id == ItemId.RECIPE_RANDOM_NORMAL || item.Id == ItemId.RECIPE_RANDOM_MASTERY)
                {
                    item = new Item(GetUnclaimedRecipe(item.Id));
                }
                if (item.Id != 0)
                {
                    PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(item.Save);
                    Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, EventLogger.LogEvent.CONTACT_ITEM, EncodeContactItemData(warbandContactData.Id, item.Id, (item.Id != ItemId.GOLD) ? ((int)item.QualityData.Id) : item.Amount));
                }
            }
        }
    }

    public int EncodeContactItemData(WarbandContactId contactId, ItemId itemId, int itemQualityId)
    {
        int num = (int)contactId;
        num |= (int)itemId << 8;
        return num | (itemQualityId << 24);
    }

    public void DecodeContactItemData(int value, out WarbandContactId contactId, out ItemId itemId, out int itemQualityId)
    {
        contactId = (WarbandContactId)(value & 0xFF);
        itemId = (ItemId)((value >> 8) & 0xFFF);
        itemQualityId = ((value >> 24) & 0xFF);
    }

    private void ApplyEnchantments(List<WarbandEnchantment> enchants)
    {
        for (int i = 0; i < enchants.Count; i++)
        {
            ApplyEnchantmentAttributes(enchants[i].Attributes);
            ApplyEnchantmentModifiers(enchants[i]);
        }
    }

    private void ApplyEnchantmentAttributes(List<WarbandEnchantmentAttributeData> attributeModifiers)
    {
        for (int i = 0; i < attributeModifiers.Count; i++)
        {
            AddToAttribute(attributeModifiers[i].WarbandAttributeId, attributeModifiers[i].Modifier);
        }
    }

    private void ApplyEnchantmentModifiers(WarbandEnchantment enchantment)
    {
        for (int i = 0; i < enchantment.SearchDensityModifiers.Count; i++)
        {
            WarbandEnchantmentSearchDensityModifierData warbandEnchantmentSearchDensityModifierData = enchantment.SearchDensityModifiers[i];
            int procMissionRatingId = (int)warbandEnchantmentSearchDensityModifierData.ProcMissionRatingId;
            int searchDensityId = (int)warbandEnchantmentSearchDensityModifierData.SearchDensityId;
            if (SearchDensityModifiers[procMissionRatingId].ContainsKey(searchDensityId))
            {
                Dictionary<int, int> dictionary;
                Dictionary<int, int> dictionary2 = dictionary = SearchDensityModifiers[procMissionRatingId];
                int key;
                int key2 = key = searchDensityId;
                key = dictionary[key];
                dictionary2[key2] = key + warbandEnchantmentSearchDensityModifierData.Modifier;
            }
            else
            {
                SearchDensityModifiers[procMissionRatingId].Add(searchDensityId, warbandEnchantmentSearchDensityModifierData.Modifier);
            }
        }
        for (int j = 0; j < enchantment.WyrdStoneDensityModifiers.Count; j++)
        {
            WarbandEnchantmentWyrdstoneDensityModifierData warbandEnchantmentWyrdstoneDensityModifierData = enchantment.WyrdStoneDensityModifiers[j];
            int procMissionRatingId2 = (int)warbandEnchantmentWyrdstoneDensityModifierData.ProcMissionRatingId;
            int wyrdstoneDensityId = (int)warbandEnchantmentWyrdstoneDensityModifierData.WyrdstoneDensityId;
            if (WyrdstoneDensityModifiers[procMissionRatingId2].ContainsKey(wyrdstoneDensityId))
            {
                Dictionary<int, int> dictionary3;
                Dictionary<int, int> dictionary4 = dictionary3 = WyrdstoneDensityModifiers[procMissionRatingId2];
                int key;
                int key3 = key = wyrdstoneDensityId;
                key = dictionary3[key];
                dictionary4[key3] = key + warbandEnchantmentWyrdstoneDensityModifierData.Modifier;
            }
            else
            {
                WyrdstoneDensityModifiers[procMissionRatingId2].Add(wyrdstoneDensityId, warbandEnchantmentWyrdstoneDensityModifierData.Modifier);
            }
        }
        for (int k = 0; k < enchantment.MarketEventModifiers.Count; k++)
        {
            MarketEventModifiers.Add(enchantment.MarketEventModifiers[k].MarketEventId, enchantment.MarketEventModifiers[k].Modifier);
        }
    }

    public int GetAttribute(WarbandAttributeId attributeId)
    {
        return attributes[(int)attributeId];
    }

    public float GetPercAttribute(WarbandAttributeId attributeId)
    {
        return (float)attributes[(int)attributeId] / 100f;
    }

    public void SetAttribute(WarbandAttributeId attributeId, int value)
    {
        attributes[(int)attributeId] = value;
        CheckStat(attributeId);
    }

    public void AddToAttribute(WarbandAttributeId attributeId, int increment)
    {
        attributes[(int)attributeId] += increment;
        WarbandAttributeData warbandAttributeData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandAttributeData>((int)attributeId);
        if (warbandAttributeData.CheckAchievement)
        {
            int num = attributes[(int)attributeId];
            warbandSave.stats.stats[(int)attributeId] = num;
            PandoraSingleton<GameManager>.Instance.Profile.CheckAchievement(this, attributeId, num);
            PandoraSingleton<GameManager>.Instance.Profile.AddToStat(attributeId, increment);
        }
    }

    public void CheckStat(WarbandAttributeId attributeId)
    {
    }

    public WarbandSave GetWarbandSave()
    {
        return warbandSave;
    }

    public bool IsUnitTypeUnlocked(UnitTypeId unitType)
    {
        List<WarbandRankJoinUnitTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankJoinUnitTypeData>("fk_unit_type_id", unitType.ToIntString());
        if (list.Count == 1 && (int)list[0].WarbandRankId <= Rank)
        {
            return true;
        }
        return false;
    }

    public int GetActiveUnitIdCount(UnitId unitId, List<int> excludeSlots = null)
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (IsActiveWarbandSlot(Units[i].UnitSave.warbandSlotIndex) && Units[i].Id == unitId && (excludeSlots == null || !excludeSlots.Contains(Units[i].UnitSave.warbandSlotIndex)))
            {
                num++;
            }
        }
        return num;
    }

    public int GetRating()
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].Active)
            {
                int rating = Units[i].GetRating();
                num += rating;
            }
        }
        return num;
    }

    public int GetSkirmishRating()
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (IsActiveWarbandSlot(Units[i].UnitSave.warbandSlotIndex))
            {
                int rating = Units[i].GetRating();
                num += rating;
            }
        }
        return num;
    }

    public int GetCartSize()
    {
        return GetWarbandRankData().CartSize;
    }

    public WarbandRankData GetWarbandRankData()
    {
        return PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(Rank);
    }

    public WarbandRankData GetNextWarbandRankData()
    {
        return PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(Rank + 1);
    }

    public int GetMaxRank()
    {
        if (maxRank == -1)
        {
            List<WarbandRankData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>();
            for (int i = 0; i < list.Count; i++)
            {
                if (maxRank < list[i].Rank)
                {
                    maxRank = list[i].Rank;
                }
            }
        }
        return maxRank;
    }

    public void RefreshOutsiders()
    {
        List<UnitTypeId> list = new List<UnitTypeId>(OutsidersTypeIds);
        int num = 0;
        Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
        for (int num2 = warbandSave.outsiders.Count - 1; num2 >= 0; num2--)
        {
            if (!warbandSave.outsiders[num2].isFreeOutsider)
            {
                if ((float)localTyche.Rand(0, 100) < Constant.GetFloat(ConstantId.HIRE_UNIT_REMOVE_OUTSIDER_RATIO) * 100f)
                {
                    warbandSave.outsiders.RemoveAt(num2);
                    Outsiders.RemoveAt(num2);
                }
                else
                {
                    num++;
                    for (int i = 0; i < list.Count; i++)
                    {
                        UnitTypeId unitTypeId = Outsiders[num2].Data.UnitTypeId;
                        if (list[i] == unitTypeId || (list[i] == UnitTypeId.HERO_1 && (unitTypeId == UnitTypeId.HERO_2 || unitTypeId == UnitTypeId.HERO_3)))
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
        bool flag = false;
        List<UnitData> list2 = new List<UnitData>();
        for (int j = num; j < GetAttribute(WarbandAttributeId.OUTSIDERS_COUNT); j++)
        {
            if (!((float)localTyche.Rand(0, 100) < Constant.GetFloat(ConstantId.HIRE_UNIT_ADD_OUTSIDER_RATIO) * 100f))
            {
                continue;
            }
            UnitTypeId unitTypeId2 = list[0];
            list.RemoveAt(0);
            if (unitTypeId2 == UnitTypeId.IMPRESSIVE && !IsUnitTypeUnlocked(UnitTypeId.IMPRESSIVE))
            {
                unitTypeId2 = UnitTypeId.HERO_1;
            }
            for (int k = 0; k < HireableOutsiderUnitIds.Count; k++)
            {
                UnitData unitData = HireableOutsiderUnitIds[k];
                UnitTypeId unitTypeId3 = unitData.UnitTypeId;
                if (unitTypeId3 == unitTypeId2 || (unitTypeId2 == UnitTypeId.HERO_1 && (unitTypeId3 == UnitTypeId.HERO_2 || unitTypeId3 == UnitTypeId.HERO_3)))
                {
                    list2.Add(unitData);
                }
            }
            UnitData unitData2 = list2[localTyche.Rand(0, list2.Count)];
            list2.Clear();
            int unitRank = localTyche.Rand(1, HireableUnitTypeRank[(int)unitData2.UnitTypeId] + 1);
            Unit unit = Unit.GenerateHireUnit(unitData2.Id, Rank, unitRank);
            unit.UnitSave.isOutsider = true;
            Outsiders.Add(unit);
            warbandSave.outsiders.Add(unit.UnitSave);
            flag = true;
        }
        if (flag)
        {
            PandoraSingleton<Pan>.Instance.Narrate("new_hiredsword");
        }
    }

    public void HireOutsider(Unit unit)
    {
        bool flag = warbandSave.outsiders.Remove(unit.UnitSave);
        Outsiders.Remove(unit);
        HireUnit(unit);
    }

    public void HireUnit(Unit unit)
    {
        warbandSave.units.Add(unit.UnitSave);
        Units.Add(unit);
        unit.Logger.AddHistory(warbandSave.currentDate, EventLogger.LogEvent.HIRE, 0);
        IncrementHireStat(unit);
    }

    public bool IsSlotAvailable(int slotIdx)
    {
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].UnitSave.warbandSlotIndex == slotIdx)
            {
                return false;
            }
        }
        return true;
    }

    private void IncrementHireStat(Unit unit)
    {
        switch (unit.GetUnitTypeId())
        {
            case UnitTypeId.LEADER:
                AddToAttribute(WarbandAttributeId.HIRE_LEADER, 1);
                break;
            case UnitTypeId.HENCHMEN:
                AddToAttribute(WarbandAttributeId.HIRE_HENCHMEN, 1);
                break;
            case UnitTypeId.IMPRESSIVE:
                AddToAttribute(WarbandAttributeId.HIRE_IMPRESSIVE, 1);
                break;
            case UnitTypeId.HERO_1:
            case UnitTypeId.HERO_2:
            case UnitTypeId.HERO_3:
                AddToAttribute(WarbandAttributeId.HIRE_HERO, 1);
                break;
        }
        AddToAttribute(WarbandAttributeId.TOTAL_HIRE_UNIT, 1);
        if (unit.UnitSave.isOutsider)
        {
            AddToAttribute(WarbandAttributeId.TOTAL_HIRE_OUTSIDER, 1);
        }
    }

    public List<Item> Disband(Unit unit)
    {
        warbandSave.units.Remove(unit.UnitSave);
        warbandSave.oldUnits.Add(unit.UnitSave.stats);
        Units.Remove(unit);
        return unit.UnequipAllItems();
    }

    public int GetTotalUpkeepOwned()
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            num += Units[i].GetUpkeepOwned();
        }
        return num;
    }

    public void PayAllUpkeepOwned()
    {
        for (int i = 0; i < Units.Count; i++)
        {
            Units[i].PayUpkeepOwned();
        }
    }

    public int GetTotalTreatmentOwned()
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            num += GetUnitTreatmentCost(Units[i]);
        }
        return num;
    }

    public void PayAllTreatmentOwned()
    {
        for (int i = 0; i < Units.Count; i++)
        {
            if (!Units[i].UnitSave.injuryPaid)
            {
                Units[i].TreatmentPaid();
            }
        }
    }

    public string GetBannerName()
    {
        return GetBannerName(Id);
    }

    public static string GetBannerName(WarbandId warbandId)
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)warbandId);
        string wagon = warbandData.Wagon;
        wagon = wagon.Substring(5);
        return "banner" + wagon;
    }

    public string GetIdolName()
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)Id);
        string wagon = warbandData.Wagon;
        if (!string.IsNullOrEmpty(wagon))
        {
            wagon = wagon.Substring(5);
            return "idol" + wagon;
        }
        return string.Empty;
    }

    public string GetMapName()
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)Id);
        string wagon = warbandData.Wagon;
        wagon = wagon.Substring(5);
        return "map_table" + wagon;
    }

    public string GetMapPawnName()
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)Id);
        string wagon = warbandData.Wagon;
        wagon = wagon.Substring(5);
        return "map_pawn" + wagon;
    }

    public static Sprite GetIcon(WarbandId warbandId)
    {
        Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>($"warband/{warbandId.ToLowerString()}_large");
        if (sprite == null)
        {
            PandoraDebug.LogWarning("Cannot load warband icon : warband/" + warbandId.ToLowerString() + "_large");
        }
        return sprite;
    }

    public static Sprite GetFlagIcon(WarbandId warbandId, bool defeated = false)
    {
        if (defeated)
        {
            return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("flags/flag_" + warbandId.ToLowerString() + "_defeated");
        }
        return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("flags/flag_" + warbandId.ToLowerString());
    }

    public static string GetLocalizedName(WarbandId warbandId)
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + warbandId.ToLowerString());
    }

    public WarbandRankSlotData GetWarbandSlots()
    {
        return PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankSlotData>("fk_warband_rank_id", Rank.ToString())[0];
    }

    public int GetNbMaxReserveSlot()
    {
        WarbandRankSlotData warbandSlots = GetWarbandSlots();
        return warbandSlots.Reserve;
    }

    public int GetNbReserveUnits()
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].UnitSave.warbandSlotIndex >= 12)
            {
                num = ((!Units[i].IsImpressive) ? (num + 1) : (num + 2));
            }
        }
        return num;
    }

    public int GetNbMaxActiveSlots()
    {
        WarbandRankSlotData warbandSlots = GetWarbandSlots();
        return warbandSlots.Leader + warbandSlots.Hero + warbandSlots.Impressive + warbandSlots.Henchman;
    }

    public int GetNbActiveUnits(bool impressiveCountFor2 = true)
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].Active)
            {
                num = ((!impressiveCountFor2 || !Units[i].IsImpressive) ? (num + 1) : (num + 2));
            }
        }
        return num;
    }

    public int GetNbInactiveUnits(bool impressiveCountFor2 = true)
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].GetActiveStatus() != 0)
            {
                num = ((!impressiveCountFor2 || !Units[i].IsImpressive) ? (num + 1) : (num + 2));
            }
        }
        return num;
    }

    public bool CanHireMoreUnit(bool isImpressive)
    {
        if (GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            return false;
        }
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            num = ((!Units[i].IsImpressive) ? (num + 1) : (num + 2));
        }
        int num2 = GetNbMaxActiveSlots() + GetNbMaxReserveSlot();
        return (!isImpressive) ? (num + 1 <= num2) : (num + 2 <= num2);
    }

    public int GetNbWarbandSlot(UnitTypeId warbandSlotId)
    {
        WarbandRankSlotData warbandSlots = GetWarbandSlots();
        int result = 0;
        switch (warbandSlotId)
        {
            case UnitTypeId.LEADER:
                result = warbandSlots.Leader;
                break;
            case UnitTypeId.IMPRESSIVE:
                result = warbandSlots.Impressive;
                break;
            case UnitTypeId.HERO_1:
            case UnitTypeId.HERO_2:
            case UnitTypeId.HERO_3:
                result = warbandSlots.Hero;
                break;
            case UnitTypeId.HENCHMEN:
                result = warbandSlots.Henchman;
                break;
        }
        return result;
    }

    public bool CanPlaceUnitAt(Unit unit, int toIndex)
    {
        UnitTypeId unitTypeId = unit.GetUnitTypeId();
        bool result = false;
        if (toIndex < 20)
        {
            switch (unitTypeId)
            {
                case UnitTypeId.LEADER:
                    result = (toIndex == 2 || toIndex >= 12);
                    break;
                case UnitTypeId.IMPRESSIVE:
                    if (toIndex >= 5 && toIndex < 7 && (toIndex - 5) % 2 == 0)
                    {
                        result = true;
                    }
                    else if (toIndex >= 12 && (toIndex - 12) % 2 == 0)
                    {
                        result = true;
                    }
                    break;
                case UnitTypeId.HERO_1:
                case UnitTypeId.HERO_2:
                case UnitTypeId.HERO_3:
                    result = ((toIndex >= 3 && toIndex < 7) || toIndex >= 12);
                    break;
                case UnitTypeId.HENCHMEN:
                    result = (toIndex >= 3);
                    break;
            }
        }
        else
        {
            result = true;
        }
        return result;
    }

    public bool IsUnitCountExceeded(Unit unit, int excludeSlot = -1)
    {
        tempList[0] = excludeSlot;
        return GetActiveUnitIdCount(unit.Id, tempList) >= unit.Data.MaxCount;
    }

    public bool IsActiveWarbandSlot(int warbandSlotIndex)
    {
        return warbandSlotIndex >= 2 && warbandSlotIndex < 12;
    }

    public void FindSuitableSlot(Unit unit, bool checkCurrent)
    {
        if (checkCurrent && unit.UnitSave.warbandSlotIndex >= 0 && unit.UnitSave.warbandSlotIndex < 20)
        {
            Unit unitAtWarbandSlot = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitAtWarbandSlot(unit.UnitSave.warbandSlotIndex);
            if (unitAtWarbandSlot == null || unitAtWarbandSlot == unit || (unit.IsImpressive && PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitAtWarbandSlot(unit.UnitSave.warbandSlotIndex + 1) == null))
            {
                return;
            }
        }
        WarbandRankSlotData warbandSlots = GetWarbandSlots();
        int emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.RESERVE, warbandSlots.Reserve, unit.IsImpressive);
        if (emptyWarbandSlotIndex == -1)
        {
            switch (unit.GetUnitTypeId())
            {
                case UnitTypeId.LEADER:
                    emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.LEADER, warbandSlots.Leader, unit.IsImpressive);
                    break;
                case UnitTypeId.IMPRESSIVE:
                    emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HERO_IMPRESSIVE, warbandSlots.Impressive, unit.IsImpressive);
                    break;
                case UnitTypeId.HERO_1:
                case UnitTypeId.HERO_2:
                case UnitTypeId.HERO_3:
                    emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HERO, warbandSlots.Hero, unit.IsImpressive);
                    if (emptyWarbandSlotIndex == -1)
                    {
                        emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HERO_IMPRESSIVE, warbandSlots.Impressive, unit.IsImpressive);
                    }
                    break;
                case UnitTypeId.HENCHMEN:
                    emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HENCHMEN, warbandSlots.Henchman, unit.IsImpressive);
                    if (emptyWarbandSlotIndex == -1)
                    {
                        emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HERO, warbandSlots.Hero, unit.IsImpressive);
                    }
                    if (emptyWarbandSlotIndex == -1)
                    {
                        emptyWarbandSlotIndex = GetEmptyWarbandSlotIndex(WarbandSlotTypeId.HERO_IMPRESSIVE, warbandSlots.Impressive, unit.IsImpressive);
                    }
                    break;
            }
        }
        if (emptyWarbandSlotIndex != -1)
        {
            unit.UnitSave.warbandSlotIndex = emptyWarbandSlotIndex;
        }
    }

    private int GetEmptyWarbandSlotIndex(WarbandSlotTypeId warbandSlotTypeId, int slotCount, bool isImpressive)
    {
        for (int i = (int)warbandSlotTypeId; i < (int)(warbandSlotTypeId + slotCount); i++)
        {
            Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(i);
            if (unitAtWarbandSlot == null)
            {
                if (!isImpressive)
                {
                    return i;
                }
                if ((int)(i - warbandSlotTypeId) % 2 == 0 && GetUnitAtWarbandSlot(i + 1) == null)
                {
                    return i;
                }
            }
            else if (unitAtWarbandSlot.IsImpressive)
            {
                i++;
            }
        }
        return -1;
    }

    public Unit GetUnitAtWarbandSlot(int index, bool includeUnavailable = false)
    {
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].UnitSave.warbandSlotIndex == index && (includeUnavailable || Units[i].GetActiveStatus() == UnitActiveStatusId.AVAILABLE))
            {
                return Units[i];
            }
        }
        return null;
    }

    public List<ItemId> GetMarketAdditionalItems(ItemCategoryId itemCategoryId)
    {
        addtionnalMarketItems.Clear();
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)itemCategoryId;
        List<ItemTypeData> list = instance.InitData<ItemTypeData>("fk_item_category_id", num.ToString());
        for (int i = 0; i < AddtionnalMarketItems.Count; i++)
        {
            ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>((int)AddtionnalMarketItems[i]);
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].Id == itemData.ItemTypeId)
                {
                    addtionnalMarketItems.Add(itemData.Id);
                    break;
                }
            }
        }
        return addtionnalMarketItems;
    }

    public List<ItemId> GetAllowedItemIds()
    {
        allowedItemIds.Clear();
        for (int i = 0; i < HireableUnitIds.Count; i++)
        {
            List<ItemUnitData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemUnitData>("fk_unit_id", ((int)HireableUnitIds[i]).ToString());
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].ItemId != 0 && allowedItemIds.IndexOf(list[j].ItemId, ItemIdComparer.Instance) == -1)
                {
                    allowedItemIds.Add(list[j].ItemId);
                }
            }
        }
        return allowedItemIds;
    }

    public WarbandData GetNextNotFacedWarband(Tyche tyche)
    {
        List<WarbandData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("basic", "1");
        List<WarbandData> list2 = new List<WarbandData>(list);
        if (warbandSave.warbandFaced == 0)
        {
            for (int i = 0; i < list2.Count; i++)
            {
                warbandSave.warbandFaced |= 1 << i;
            }
        }
        else
        {
            for (int num = list2.Count - 1; num >= 0; num--)
            {
                if (((warbandSave.warbandFaced >> num) & 1) == 0)
                {
                    list2.RemoveAt(num);
                }
            }
        }
        int index = tyche.Rand(0, list2.Count);
        int num2 = list.IndexOf(list2[index]);
        warbandSave.warbandFaced &= ~(1 << num2);
        return list[num2];
    }

    public List<Unit> GetUnavailableUnits()
    {
        List<Unit> list = new List<Unit>();
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].GetActiveStatus() != 0 || Units[i].UnitSave.warbandSlotIndex >= 20 || Units[i].UnitSave.warbandSlotIndex == -1)
            {
                list.Add(Units[i]);
            }
        }
        return list;
    }

    public int GetScoutCost(ScoutPriceData scout)
    {
        return Mathf.Max(0, (int)((float)(scout.Price * GetAttribute(WarbandAttributeId.SCOUT_COST_PERC)) / 100f));
    }

    public int GetUnitHireCost(Unit unit)
    {
        return Mathf.Max(0, (int)((float)(unit.GetHireCost() * GetAttribute(WarbandAttributeId.HIRE_COST_PERC)) / 100f));
    }

    public int GetUnitTreatmentCost(Unit unit)
    {
        return Mathf.Max(0, unit.GetTreatmentCost() + GetAttribute(WarbandAttributeId.UPKEEP_WOUNDED));
    }

    public int GetItemSellPrice(Item item)
    {
        return Mathf.Clamp((int)((float)(item.PriceSold * GetAttribute(WarbandAttributeId.SELL_PRICE_PERC)) / 100f), 0, GetItemBuyPrice(item));
    }

    public int GetItemBuyPrice(Item item)
    {
        return Mathf.Max(0, (int)((float)(item.PriceBuy * GetAttribute(WarbandAttributeId.BUY_COST_PERC)) / 100f));
    }

    public int GetRuneMarkBuyPrice(RuneMark runeMark)
    {
        return Mathf.Max(0, (int)((float)(runeMark.Cost * GetAttribute(WarbandAttributeId.BUY_COST_PERC)) / 100f));
    }

    public int GetSkillLearnPrice(SkillData skillData, Unit unit)
    {
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            return 0;
        }
        SkillLineData baseSkillLine = SkillHelper.GetBaseSkillLine(skillData, unit.Id);
        if (baseSkillLine.WarbandAttributeIdPriceModifier != 0)
        {
            return (int)((float)(skillData.Cost * (GetAttribute(WarbandAttributeId.SKILL_SPELL_LEARN_COST_PERC) + GetAttribute(baseSkillLine.WarbandAttributeIdPriceModifier))) / 100f);
        }
        return Mathf.Max(0, (int)((float)(skillData.Cost * GetAttribute(WarbandAttributeId.SKILL_SPELL_LEARN_COST_PERC)) / 100f));
    }

    public void SetHideoutTutoShown(HideoutManager.HideoutTutoType type)
    {
        warbandSave.hideoutTutos |= (uint)type;
    }

    public bool HasShownHideoutTuto(HideoutManager.HideoutTutoType type)
    {
        return ((int)warbandSave.hideoutTutos & (int)type) != 0;
    }

    public List<UnitSave> GetExhibitionUnits()
    {
        return new List<UnitSave>();
    }

    public bool IsSkirmishAvailable(out string reason)
    {
        reason = string.Empty;
        if (warbandSave.currentDate == Constant.GetInt(ConstantId.CAL_DAY_START) && Units.Count == 1 && warbandSave.scoutsSent < 0)
        {
            reason = "na_hideout_new_game";
            return false;
        }
        if (!HasLeader(needToBeActive: false))
        {
            reason = "na_hideout_leader";
            return false;
        }
        if (GetUnitsCount(needToBeActive: false) < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
        {
            reason = "na_hideout_min_unit";
            return false;
        }
        return true;
    }

    public bool IsContestAvailable(out string reason)
    {
        reason = string.Empty;
        if (warbandSave.lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            reason = "na_hideout_late_shipment_count";
            return false;
        }
        if (warbandSave.scoutsSent < 0)
        {
            reason = "na_hideout_post_mission";
            return false;
        }
        if (!HasLeader(needToBeActive: true))
        {
            reason = "na_hideout_active_leader";
            return false;
        }
        if (GetUnitsCount(needToBeActive: true) < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
        {
            reason = "na_hideout_min_active_unit";
            return false;
        }
        return true;
    }

    public bool HasLeader(bool needToBeActive)
    {
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].GetUnitTypeId() == UnitTypeId.LEADER && (!needToBeActive || Units[i].GetActiveStatus() == UnitActiveStatusId.AVAILABLE) && Units[i].UnitSave.warbandSlotIndex < 12)
            {
                return true;
            }
        }
        return false;
    }

    public int GetUnitsCount(bool needToBeActive)
    {
        int num = 0;
        for (int i = 0; i < Units.Count; i++)
        {
            if ((!needToBeActive || Units[i].GetActiveStatus() == UnitActiveStatusId.AVAILABLE) && Units[i].UnitSave.warbandSlotIndex < 12)
            {
                num++;
            }
        }
        return num;
    }
}
