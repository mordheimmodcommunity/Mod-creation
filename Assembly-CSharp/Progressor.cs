using System.Collections.Generic;
using UnityEngine;

public class Progressor
{
    private const int BASE_INJURY_COUNT = 1;

    private const int MULTIPLE_INJURY_COUNT = 3;

    private static readonly InjuryId[] INJURY_EXCLUDES = new InjuryId[3]
    {
        InjuryId.DEAD,
        InjuryId.MULTIPLE_INJURIES,
        InjuryId.FULL_RECOVERY
    };

    private static readonly UnitTypeId[] LOWEST_RANK_UNIT_TYPES = new UnitTypeId[5]
    {
        UnitTypeId.HERO_3,
        UnitTypeId.HERO_2,
        UnitTypeId.HERO_1,
        UnitTypeId.HENCHMEN,
        UnitTypeId.LEADER
    };

    private static readonly UnitTypeId[] HIGHEST_RANK_UNIT_TYPES = new UnitTypeId[5]
    {
        UnitTypeId.HERO_1,
        UnitTypeId.HERO_2,
        UnitTypeId.HERO_3,
        UnitTypeId.HENCHMEN,
        UnitTypeId.LEADER
    };

    public void UpdateUnitStats(MissionEndUnitSave endUnit, Unit unit)
    {
        foreach (KeyValuePair<int, int> stat in endUnit.unitSave.stats.stats)
        {
            SetUnitStat(unit, (AttributeId)stat.Key, stat.Value);
        }
    }

    public void EndGameUnitProgress(MissionEndUnitSave endUnit, Unit unit)
    {
        for (int num = endUnit.items.Count - 1; num >= 6; num--)
        {
            if (endUnit.items[num].TypeData.Id == ItemTypeId.QUEST_ITEM)
            {
                endUnit.items[num] = new Item(ItemId.NONE);
            }
        }
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        if (endMission.VictoryType == VictoryTypeId.LOSS && endUnit.status == UnitStateId.OUT_OF_ACTION)
        {
            CalculateCostofLosing(endUnit);
        }
        for (int i = 0; i < unit.Items.Count; i++)
        {
            endUnit.lostItems.Add(new Item(ItemId.NONE));
        }
        for (int num2 = unit.Items.Count - 1; num2 >= 0; num2--)
        {
            bool flag = false;
            for (int j = 0; j < endUnit.items.Count; j++)
            {
                if (flag)
                {
                    break;
                }
                if (endUnit.items[j].IsSame(unit.Items[num2]))
                {
                    flag = true;
                    endUnit.items.RemoveAt(j);
                }
            }
            if (!flag)
            {
                List<Item> list = unit.EquipItem((UnitSlotId)num2, new Item(ItemId.NONE));
                for (int k = 1; k < list.Count; k++)
                {
                    PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(list[k].Save);
                }
            }
        }
        if (endMission.missionSave.VictoryTypeId != 2 || endMission.VictoryType != 0)
        {
            for (int l = 0; l < endUnit.items.Count; l++)
            {
                endMission.wagonItems.AddItem(endUnit.items[l].Save);
            }
        }
        for (int m = 0; m < endUnit.enchantments.Count; m++)
        {
            EndUnitEnchantment endUnitEnchantment = endUnit.enchantments[m];
            unit.AddEnchantment(endUnitEnchantment.enchantId, unit, original: false);
        }
        unit.SetStatus(endUnit.status);
        endUnit.dead = !CalculateInjuries(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, unit, endUnit);
        PandoraDebug.LogDebug("Unit Dead After Injuries = " + endUnit.dead);
        if (endUnit.injuries.Count > 0)
        {
            AddInjuryTime(unit, endUnit.injuries[endUnit.injuries.Count - 1].Id);
        }
        for (int n = 0; n < endUnit.enchantments.Count; n++)
        {
            EndUnitEnchantment endUnitEnchantment2 = endUnit.enchantments[n];
            unit.RemoveEnchantments(endUnitEnchantment2.enchantId);
        }
        unit.SetStatus(UnitStateId.NONE);
        if (endUnit.dead)
        {
            for (int num3 = 0; num3 < unit.Items.Count; num3++)
            {
                if (unit.Items[num3].Id != 0)
                {
                    PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(unit.Items[num3].Save);
                }
            }
            unit.Items.Clear();
        }
        if (!endUnit.dead)
        {
            CalculateXP(endUnit, unit);
            for (int num4 = 0; num4 < endUnit.mutations.Count; num4++)
            {
                unit.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, EventLogger.LogEvent.MUTATION, (int)endUnit.mutations[num4].Data.Id);
            }
            if (endUnit.mutations.Count > 0)
            {
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.MUTATIONS, 1);
            }
        }
        if (!endUnit.dead)
        {
            UpdateUpkeep(endUnit, unit, PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.VictoryType);
        }
        UpdateUnitStats(endUnit, unit);
        unit.UpdateAttributes();
    }

    public void CalculateCostofLosing(MissionEndUnitSave endUnit)
    {
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.VictoryType != 0)
        {
            return;
        }
        List<CostOfLosingData> datas = PandoraSingleton<DataFactory>.Instance.InitData<CostOfLosingData>();
        Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
        CostOfLosingData randomRatio = CostOfLosingData.GetRandomRatio(datas, localTyche);
        endUnit.costOfLosingId = randomRatio.Id;
        for (int num = endUnit.items.Count - 1; num >= 6; num--)
        {
            if (endUnit.items[num].Id != 0)
            {
                endUnit.items[num] = new Item(ItemId.NONE);
            }
        }
        if (randomRatio.MainWeapons)
        {
            if (endUnit.items[2].Id != 0)
            {
                endUnit.items[2] = new Item(ItemId.NONE);
            }
            if (endUnit.items[3].Id != 0)
            {
                endUnit.items[3] = new Item(ItemId.NONE);
            }
        }
        if (randomRatio.SecondaryWeapons)
        {
            if (endUnit.items[4].Id != 0)
            {
                endUnit.items[4] = new Item(ItemId.NONE);
            }
            if (endUnit.items[5].Id != 0)
            {
                endUnit.items[5] = new Item(ItemId.NONE);
            }
        }
        if (randomRatio.Armor && endUnit.items[1].Id != 0)
        {
            endUnit.items[1] = new Item(ItemId.NONE);
        }
        if (randomRatio.Helmet && endUnit.items[0].Id != 0)
        {
            endUnit.items[0] = new Item(ItemId.NONE);
        }
        if (randomRatio.OpenWound)
        {
            EndUnitEnchantment item = default(EndUnitEnchantment);
            item.enchantId = EnchantmentId.OPEN_WOUND;
            endUnit.enchantments.Add(item);
        }
    }

    public bool CalculateInjuries(int currentDate, Unit unit, MissionEndUnitSave endUnit)
    {
        endUnit.injuries.Clear();
        bool flag = true;
        List<Item> list = new List<Item>();
        if (unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            if (unit.HasEnchantment(EnchantmentId.OPEN_WOUND))
            {
                InjuryData injuryData = PandoraSingleton<DataFactory>.Instance.InitData<InjuryData>(31);
                flag = unit.AddInjury(injuryData, currentDate, list);
                endUnit.injuries.Add(injuryData);
            }
        }
        else
        {
            List<InjuryId> list2 = new List<InjuryId>();
            int num = 1;
            while (num > 0 && flag)
            {
                InjuryData injuryData2 = RollInjury(list2, unit);
                if (injuryData2.Id == InjuryId.MULTIPLE_INJURIES)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.MULTIPLE_INJURIES);
                    list2.AddRange(INJURY_EXCLUDES);
                    num = 3;
                    continue;
                }
                endUnit.injuries.Add(injuryData2);
                if (injuryData2.Id != InjuryId.FULL_RECOVERY)
                {
                    flag = unit.AddInjury(injuryData2, currentDate, list);
                    if (flag)
                    {
                        PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.MY_TOTAL_INJURIES, 1);
                    }
                }
                num--;
                list2.Add(injuryData2.Id);
            }
        }
        PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list);
        PandoraDebug.LogDebug("CalculateInjuries return = " + flag + " for " + unit.Name);
        return flag;
    }

    public InjuryData RollInjury(List<InjuryId> excludes, Unit unit)
    {
        Dictionary<InjuryId, int> injuryModifiers = unit.GetInjuryModifiers();
        List<InjuryData> possibleInjuries = unit.GetPossibleInjuries(excludes, unit, injuryModifiers);
        return InjuryData.GetRandomRatio(possibleInjuries, PandoraSingleton<GameManager>.Instance.LocalTyche, injuryModifiers);
    }

    private void CalculateXP(MissionEndUnitSave endUnit, Unit unit)
    {
        if (unit.IsMaxRank())
        {
            endUnit.isMaxRank = true;
            return;
        }
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        int num = 0;
        endUnit.XPs.Clear();
        endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_SURVIVED), "end_game_survive"));
        num += Constant.GetInt(ConstantId.UNIT_XP_SURVIVED);
        if (endMission.primaryObjectiveCompleted)
        {
            endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_OBJECTIVE), "end_game_objective"));
            num += Constant.GetInt(ConstantId.UNIT_XP_OBJECTIVE);
            if (endMission.won)
            {
                num += Constant.GetInt(ConstantId.UNIT_XP_WINNING);
                endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_WINNING), "mission_victory_decisive"));
            }
        }
        if (endMission.playerMVUIdx == endUnit.unitSave.warbandSlotIndex)
        {
            num += Constant.GetInt(ConstantId.UNIT_XP_MVU);
            endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_MVU), "menu_mvu"));
        }
        int underdogBonus = GetUnderdogBonus(endMission);
        if (underdogBonus > 0)
        {
            num += underdogBonus;
            endUnit.XPs.Add(new KeyValuePair<int, string>(underdogBonus, "end_game_underdog"));
        }
        int value = 0;
        unit.UnitSave.stats.stats.TryGetValue(141, out value);
        int num2 = endUnit.GetAttribute(AttributeId.TOTAL_KILL) - value;
        if (num2 > 0)
        {
            num += num2 * Constant.GetInt(ConstantId.UNIT_XP_OUT_OF_ACTION);
            endUnit.XPs.Add(new KeyValuePair<int, string>(num2 * Constant.GetInt(ConstantId.UNIT_XP_OUT_OF_ACTION), "end_game_ooa"));
        }
        value = 0;
        unit.UnitSave.stats.stats.TryGetValue(151, out value);
        num2 = endUnit.GetAttribute(AttributeId.TOTAL_KILL_ROAMING) - value;
        if (num2 > 0)
        {
            int num3 = num2 * Constant.GetInt(ConstantId.UNIT_XP_ROAMING_OUT_OF_ACTION);
            num += num3;
            endUnit.XPs.Add(new KeyValuePair<int, string>(num3, "end_game_ooa_roaming"));
        }
        for (int i = 0; i < endUnit.injuries.Count; i++)
        {
            if (endUnit.injuries[i].Id == InjuryId.NEAR_DEATH)
            {
                num += Constant.GetInt(ConstantId.UNIT_XP_NEAR_DEATH);
                endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_NEAR_DEATH), "injury_name_near_death"));
            }
            if (endUnit.injuries[i].Id == InjuryId.AMNESIA)
            {
                num += Constant.GetInt(ConstantId.UNIT_XP_AMNESIA);
                endUnit.XPs.Add(new KeyValuePair<int, string>(Constant.GetInt(ConstantId.UNIT_XP_AMNESIA), "injury_name_amnesia"));
            }
        }
        List<Item> list = new List<Item>();
        unit.AddXp(num, endUnit.advancements, endUnit.mutations, list, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate);
        PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list);
        if (endUnit.advancements.Count <= 0)
        {
            return;
        }
        PandoraSingleton<GameManager>.Instance.Profile.CheckAchievement(unit, AttributeId.RANK, unit.GetAttribute(AttributeId.RANK));
        UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)unit.UnitSave.rankId);
        if (unitRankData.Rank != Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            return;
        }
        bool flag = true;
        for (int j = 0; j < unit.Injuries.Count; j++)
        {
            InjuryId id = unit.Injuries[j].Data.Id;
            if (id != InjuryId.LIGHT_WOUND && id != InjuryId.FULL_RECOVERY && id != InjuryId.NEAR_DEATH && id != InjuryId.AMNESIA)
            {
                flag = false;
                break;
            }
        }
        switch (unit.GetUnitTypeId())
        {
            case UnitTypeId.MONSTER:
                break;
            case UnitTypeId.LEADER:
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.LEADER_RANK_10, 1);
                if (flag)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.LEADER_NO_INJURY);
                }
                break;
            case UnitTypeId.HERO_1:
            case UnitTypeId.HERO_2:
            case UnitTypeId.HERO_3:
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.HERO_RANK_10, 1);
                if (flag)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.HERO_NO_INJURY);
                }
                break;
            case UnitTypeId.HENCHMEN:
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.HENCHMEN_RANK_10, 1);
                if (flag)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.HENCHMEN_NO_INJURY);
                }
                break;
            case UnitTypeId.IMPRESSIVE:
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.IMPRESSIVE_RANK_10, 1);
                if (flag)
                {
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.IMPRESSIVE_NO_INJURY);
                }
                break;
        }
    }

    private int GetUnderdogBonus(MissionEndDataSave endMission)
    {
        int result = 0;
        if (endMission.ratingId > ProcMissionRatingId.NONE)
        {
            ProcMissionRatingData procMissionRatingData = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>((int)endMission.ratingId);
            result = procMissionRatingData.UnderdogXp;
        }
        return result;
    }

    private void UpdateUpkeep(MissionEndUnitSave endUnit, Unit unit, VictoryTypeId victory)
    {
        UnitCostData unitCost = unit.GetUnitCost();
        switch (victory)
        {
            case VictoryTypeId.DECISIVE:
            case VictoryTypeId.CAMPAIGN:
                unit.AddToUpkeepOwned(unitCost.DecisiveVictory);
                break;
            case VictoryTypeId.BATTLEGROUND:
            case VictoryTypeId.OBJECTIVE:
                unit.AddToUpkeepOwned(unitCost.PartialVictory);
                break;
            case VictoryTypeId.LOSS:
                unit.AddToUpkeepOwned(unitCost.Defeat);
                break;
        }
        if (victory != 0)
        {
            if (unit.GetUnitTypeId() == UnitTypeId.LEADER)
            {
                unit.AddToUpkeepOwned(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetAttribute(WarbandAttributeId.WON_UPKEEP_LEADER_MODIFIER));
            }
            else if (unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE)
            {
                unit.AddToUpkeepOwned(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetAttribute(WarbandAttributeId.WON_UPKEEP_IMPRESSIVE_MODIFIER));
            }
        }
        if (unit.UnitSave.injuredTime == 0)
        {
            int date = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + Constant.GetInt(ConstantId.UPKEEP_DAYS_WITHOUT_PAY);
            unit.Logger.AddHistory(date, EventLogger.LogEvent.LEFT, 0);
        }
    }

    public void AddInjuryTime(Unit unit, InjuryId id)
    {
        int num = Mathf.Max(1, Constant.GetInt(ConstantId.INJURY_ROLL_INTERVAL) - 1);
        if (unit.UnitSave.injuredTime > 0)
        {
            Tuple<int, EventLogger.LogEvent, int> tuple = unit.Logger.FindEventAfter(EventLogger.LogEvent.NO_TREATMENT, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + 1);
            if (tuple != null)
            {
                unit.Logger.RemoveHistory(tuple);
            }
            unit.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + num, EventLogger.LogEvent.NO_TREATMENT, (int)id);
            tuple = unit.Logger.FindEventAfter(EventLogger.LogEvent.RECOVERY, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + 1);
            if (tuple != null)
            {
                unit.Logger.RemoveHistory(tuple);
            }
            tuple = unit.Logger.FindEventAfter(EventLogger.LogEvent.LEFT, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + 1);
            if (tuple != null)
            {
                unit.Logger.RemoveHistory(tuple);
            }
        }
    }

    public void EndGameWarbandProgress(MissionEndDataSave endData, Warband warband)
    {
        warband.rankGained = false;
        if (warband.Rank < Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            warband.Xp += PandoraSingleton<DataFactory>.Instance.InitData<VictoryTypeData>((int)endData.VictoryType).WarbandExperience;
            WarbandRankData warbandRankData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(warband.Rank + 1);
            while (warband.Xp >= warbandRankData.Exp && warband.Rank < warband.GetMaxRank())
            {
                if (warband.rankGained)
                {
                    warband.Xp = warbandRankData.Exp - 1;
                    break;
                }
                warband.Rank++;
                warband.rankGained = true;
                warband.Xp -= warbandRankData.Exp;
                warbandRankData = warband.GetWarbandRankData();
            }
        }
        UpdateWarbandStats(warband);
        ProcessTrophies(warband);
        warband.UpdateAttributes();
        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GenerateHireableUnits();
    }

    public void UpdateWarbandStats(Warband warband)
    {
        MissionEndDataSave endMission = warband.GetWarbandSave().endMission;
        bool flag = true;
        for (int i = 0; i < warband.GetWarbandSave().endMission.units.Count; i++)
        {
            if (endMission.units[i].status == UnitStateId.OUT_OF_ACTION)
            {
                flag = false;
                break;
            }
        }
        if (!endMission.isVsAI)
        {
            IncrementWarbandStat(WarbandAttributeId.SKIRMISH_ATTEMPTED, 1);
            if (endMission.VictoryType != 0)
            {
                IncrementWarbandStat(WarbandAttributeId.SKIRMISH_WIN, 1);
            }
            if (endMission.VictoryType == VictoryTypeId.DECISIVE)
            {
                IncrementWarbandStat(WarbandAttributeId.SKIRMISH_DECISIVE_VICTORY, 1);
            }
            else if (endMission.VictoryType == VictoryTypeId.BATTLEGROUND)
            {
                IncrementWarbandStat(WarbandAttributeId.SKIRMISH_BATTLEGROUND_VICTORY, 1);
            }
            else if (endMission.VictoryType == VictoryTypeId.OBJECTIVE)
            {
                IncrementWarbandStat(WarbandAttributeId.SKIRMISH_OBJECTIVE_VICTORY, 1);
            }
            else
            {
                IncrementWarbandStat(WarbandAttributeId.SKIRMISH_LOST, 1);
            }
            return;
        }
        warband.AddToAttribute(WarbandAttributeId.CAMPAIGN_MISSION_ATTEMPTED, 1);
        if (endMission.won || endMission.primaryObjectiveCompleted)
        {
            IncrementWarbandStat(WarbandAttributeId.CAMPAIGN_MISSION_WIN, 1);
            int attribute = warband.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_WIN);
            if (attribute % 10 == 0)
            {
                warband.Logger.AddHistory(warband.GetWarbandSave().currentDate, EventLogger.LogEvent.MEMORABLE_CAMPAIGN_VICTORY, attribute);
            }
            warband.GetWarbandSave().winningStreak++;
            int num = warband.GetWarbandSave().winningStreak - warband.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_WIN_STREAK);
            if (num > 0)
            {
                IncrementWarbandStat(WarbandAttributeId.CAMPAIGN_MISSION_WIN_STREAK, num);
                if (warband.GetWarbandSave().winningStreak % 5 == 0)
                {
                    warband.Logger.AddHistory(warband.GetWarbandSave().currentDate, EventLogger.LogEvent.VICTORY_STREAK, warband.GetWarbandSave().winningStreak);
                }
            }
            if (endMission.VictoryType == VictoryTypeId.CAMPAIGN)
            {
                List<CampaignMissionData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(new string[2]
                {
                    "fk_warband_id",
                    "idx"
                }, new string[2]
                {
                    ((int)warband.WarbandData.Id).ToString(),
                    warband.GetWarbandSave().curCampaignIdx.ToString()
                });
                PandoraSingleton<GameManager>.Instance.Profile.CheckAchievement(list[0].Id);
                PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_SKAVEN_1);
                if (warband.GetWarbandSave().curCampaignIdx <= Constant.GetInt(ConstantId.CAMPAIGN_LAST_MISSION))
                {
                    warband.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + Constant.GetInt(ConstantId.MISSION_REWARD_DAYS), EventLogger.LogEvent.MISSION_REWARDS, warband.GetWarbandSave().curCampaignIdx);
                    PandoraSingleton<GameManager>.Instance.Profile.UpdateGameProgress(warband.Id, warband.GetWarbandSave().curCampaignIdx);
                    PandoraSingleton<Hephaestus>.Instance.UpdateGameProgress();
                    warband.GetWarbandSave().curCampaignIdx++;
                    if (list.Count > 0)
                    {
                        int date = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + list[0].Days;
                        warband.Logger.AddHistory(date, EventLogger.LogEvent.NEW_MISSION, warband.GetWarbandSave().curCampaignIdx);
                    }
                }
            }
            if (endMission.crushed)
            {
                IncrementWarbandStat(WarbandAttributeId.CAMPAIGN_MISSION_CRUSHED_VICTORY, 1);
            }
            if (flag)
            {
                IncrementWarbandStat(WarbandAttributeId.CAMPAIGN_MISSION_TOTAL_VICTORY, 1);
            }
        }
        else
        {
            IncrementWarbandStat(WarbandAttributeId.CAMPAIGN_MISSION_LOST, 1);
            warband.GetWarbandSave().winningStreak = 0;
        }
    }

    private void ProcessTrophies(Warband warband)
    {
        int num = 0;
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_SKAVEN_1))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_SKAVEN_1);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_MERC_1))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_MERC_1);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_POSSESSED_1))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_POSSESSED_1);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_SISTERS_1))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_SISTERS_1);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_WITCH_HUNTERS_1))
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_WITCH_HUNTERS_1);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_UNDEAD_1))
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_UNDEAD_1);
        }
        if (num == 4)
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_ALL_1);
        }
        num = 0;
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_SKAVEN_2))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_SKAVEN_2);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_MERC_2))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_MERC_2);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_POSSESSED_2))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_POSSESSED_2);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_SISTERS_2))
        {
            num++;
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_SISTERS_2);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_WITCH_HUNTERS_2))
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_WITCH_HUNTERS_2);
        }
        if (PandoraSingleton<GameManager>.Instance.Profile.IsAchievementUnlocked(AchievementId.STORY_UNDEAD_2))
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_UNDEAD_2);
        }
        if (num == 4)
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.STORY_ALL_2);
        }
        if (warband.rankGained && warband.Rank == Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            num = 0;
            switch (warband.Id)
            {
                case WarbandId.SKAVENS:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.SKAVEN_RANK_10);
                    break;
                case WarbandId.HUMAN_MERCENARIES:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.MERC_RANK_10);
                    break;
                case WarbandId.SISTERS_OF_SIGMAR:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.SISTERS_RANK_10);
                    break;
                case WarbandId.POSSESSED:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.POSSESSED_RANK_10);
                    break;
                case WarbandId.WITCH_HUNTERS:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.WITCH_HUNTERS_10);
                    break;
                case WarbandId.UNDEAD:
                    PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.UNDEAD_10);
                    break;
            }
            if (PandoraSingleton<Hephaestus>.Instance.IsAchievementUnlocked(Hephaestus.TrophyId.SKAVEN_RANK_10) && PandoraSingleton<Hephaestus>.Instance.IsAchievementUnlocked(Hephaestus.TrophyId.MERC_RANK_10) && PandoraSingleton<Hephaestus>.Instance.IsAchievementUnlocked(Hephaestus.TrophyId.SISTERS_RANK_10) && PandoraSingleton<Hephaestus>.Instance.IsAchievementUnlocked(Hephaestus.TrophyId.POSSESSED_RANK_10))
            {
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.ALL_RANK_10);
            }
        }
    }

    private void IncrementWarbandStat(WarbandAttributeId attributeId, int increment)
    {
        if (increment != 0)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.AddToAttribute(attributeId, increment);
        }
    }

    private void SetUnitStat(Unit unit, AttributeId attributeId, int value, bool checkWarbandStat = true)
    {
        if (value == 0 || unit == null)
        {
            return;
        }
        int attribute = unit.GetAttribute(attributeId);
        unit.SetAttribute(attributeId, value);
        if (attributeId == AttributeId.TOTAL_KILL && attribute != value && attribute / 10 != value / 10)
        {
            unit.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, EventLogger.LogEvent.MEMORABLE_KILL, value / 10 * 10);
        }
        PandoraSingleton<GameManager>.Instance.Profile.CheckAchievement(unit, attributeId, unit.GetAttribute(attributeId));
        if (!checkWarbandStat)
        {
            return;
        }
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)attributeId;
        List<WarbandAttributeData> list = instance.InitData<WarbandAttributeData>("fk_attribute_id", num.ToString());
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                IncrementWarbandStat(list[i].Id, value - attribute);
            }
        }
    }

    public void NextDayUnitProgress(Unit unit)
    {
        UnitActiveStatusId activeStatus = unit.GetActiveStatus();
        CheckForInjury(unit);
        unit.UpdateSkillTraining();
        CheckUpkeep(unit);
        if (unit.GetActiveStatus() == UnitActiveStatusId.AVAILABLE && activeStatus != 0)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.FindSuitableSlot(unit, checkCurrent: true);
        }
    }

    public void NextDayWarbandProgress()
    {
        WarbandMenuController warbandCtrlr = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr;
        WarbandSave warbandSave = warbandCtrlr.Warband.GetWarbandSave();
        warbandSave.scoutsSent = 0;
        warbandSave.missions.Clear();
        PandoraSingleton<HideoutManager>.Instance.GenerateMissions(newday: true);
        warbandCtrlr.Warband.GenerateContactItems();
        Date date = PandoraSingleton<HideoutManager>.Instance.Date;
        List<Tuple<int, EventLogger.LogEvent, int>> eventsAtDay = warbandCtrlr.Warband.Logger.GetEventsAtDay(date.CurrentDate);
        for (int i = 0; i < eventsAtDay.Count; i++)
        {
            switch (eventsAtDay[i].Item2)
            {
                case EventLogger.LogEvent.SHIPMENT_REQUEST:
                    {
                        int item = eventsAtDay[i].Item3;
                        int date2;
                        if (item == -1)
                        {
                            item = Constant.GetInt(ConstantId.FIRST_SHIPMENT_WEIGHT);
                            date2 = eventsAtDay[i].Item1 + Constant.GetInt(ConstantId.FIRST_SHIPMENT_DAYS) + warbandCtrlr.Warband.GetAttribute(WarbandAttributeId.REQUEST_TIME_MODIFIER);
                        }
                        else
                        {
                            item = eventsAtDay[i].Item3;
                            WyrdstoneShipmentData wyrdstoneShipmentData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneShipmentData>("fk_warband_rank_id", warbandSave.rank.ToString())[0];
                            date2 = eventsAtDay[i].Item1 + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(wyrdstoneShipmentData.MinDays, wyrdstoneShipmentData.MaxDays + 1) + warbandCtrlr.Warband.GetAttribute(WarbandAttributeId.REQUEST_TIME_MODIFIER);
                        }
                        warbandCtrlr.Warband.Logger.AddHistory(date2, EventLogger.LogEvent.SHIPMENT_LATE, item);
                        break;
                    }
                case EventLogger.LogEvent.SHIPMENT_DELIVERY:
                    DoDelivery(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.PrimaryFactionController, eventsAtDay[i].Item3, isShipment: true);
                    break;
                case EventLogger.LogEvent.FACTION0_DELIVERY:
                case EventLogger.LogEvent.FACTION1_DELIVERY:
                case EventLogger.LogEvent.FACTION2_DELIVERY:
                    {
                        FactionMenuController faction = null;
                        for (int j = 0; j < warbandCtrlr.factionCtrlrs.Count; j++)
                        {
                            if (warbandCtrlr.factionCtrlrs[j].Faction.GetFactionDeliveryEvent() == eventsAtDay[i].Item2)
                            {
                                faction = warbandCtrlr.factionCtrlrs[j];
                            }
                        }
                        DoDelivery(faction, eventsAtDay[i].Item3, isShipment: false);
                        break;
                    }
                case EventLogger.LogEvent.MARKET_ROTATION:
                    PandoraSingleton<HideoutManager>.Instance.Market.RefreshMarket();
                    warbandCtrlr.Warband.Logger.AddHistory(date.GetNextDay(date.WeekDay), EventLogger.LogEvent.MARKET_ROTATION, 0);
                    break;
                case EventLogger.LogEvent.RESPEC_POINT:
                    warbandSave.availaibleRespec++;
                    warbandCtrlr.Warband.Logger.AddHistory(date.CurrentDate + Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR), EventLogger.LogEvent.RESPEC_POINT, 0);
                    break;
                case EventLogger.LogEvent.OUTSIDER_ROTATION:
                    PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.RefreshOutsiders();
                    PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GenerateHireableUnits();
                    warbandCtrlr.Warband.Logger.AddHistory(date.GetNextDay(date.WeekDay), EventLogger.LogEvent.OUTSIDER_ROTATION, 0);
                    break;
            }
        }
        List<Tuple<int, EventLogger.LogEvent, int>> list = warbandCtrlr.Warband.Logger.FindEventsAfter(EventLogger.LogEvent.MISSION_REWARDS, Constant.GetInt(ConstantId.CAL_DAY_START));
        for (int k = 1; k < warbandSave.curCampaignIdx; k++)
        {
            bool flag = false;
            for (int l = 0; l < list.Count; l++)
            {
                if (list[l].Item3 == k)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                warbandCtrlr.Warband.Logger.AddHistory(date.CurrentDate, EventLogger.LogEvent.MISSION_REWARDS, k);
            }
        }
        warbandCtrlr.Warband.UpdateAttributes();
    }

    public void DoDelivery(FactionMenuController faction, int id, bool isShipment)
    {
        if (isShipment)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddGold(id);
            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.WYRDSTONE_SELL, id);
        }
        else
        {
            ShipmentSave delivery = faction.Faction.GetDelivery(id);
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddGold(delivery.gold);
            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.WYRDSTONE_SELL, delivery.gold);
            for (int i = 0; i < 5; i++)
            {
                int rankId = (delivery.rank >> 4 * i) & 0xF;
                IncrementFactionRank(faction, rankId);
            }
        }
        PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.SHIPMENTS, 1);
    }

    public void IncrementFactionRank(FactionMenuController faction, int rankId)
    {
        if (rankId > 0)
        {
            WarbandSkillId rewardWarbandSkillId = faction.Faction.GetRewardWarbandSkillId((FactionRankId)rankId);
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.AddSkill(rewardWarbandSkillId, isNew: true);
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GenerateHireableUnits();
        }
    }

    public Unit CheckLateShipment(WarbandSave warSave, WarbandMenuController ctrlr, FactionMenuController primaryFactionCtrlr)
    {
        Tuple<int, EventLogger.LogEvent, int> tuple = ctrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
        Unit unit = null;
        if (tuple != null && tuple.Item1 + 1 == PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            warSave.lateShipmentCount++;
            warSave.lastShipmentFailed = true;
            warSave.nextShipmentExtraDays = 0;
            FactionConsequenceData factionConsequenceData = PandoraSingleton<DataFactory>.Instance.InitData<FactionConsequenceData>(new string[2]
            {
                "fk_faction_id",
                "late_shipment_count"
            }, new string[2]
            {
                ((int)primaryFactionCtrlr.Faction.Data.Id).ToString(),
                warSave.lateShipmentCount.ToString()
            })[0];
            if (factionConsequenceData.FactionConsequenceTargetId != 0)
            {
                InjuryData injuryData = PandoraSingleton<DataFactory>.Instance.InitData<InjuryData>((int)factionConsequenceData.InjuryId);
                switch (factionConsequenceData.FactionConsequenceTargetId)
                {
                    case FactionConsequenceTargetId.LOWEST_RANKED_HERO:
                        {
                            for (int j = 0; j < LOWEST_RANK_UNIT_TYPES.Length; j++)
                            {
                                if (unit != null)
                                {
                                    break;
                                }
                                unit = ctrlr.GetLowestRankUnit(LOWEST_RANK_UNIT_TYPES[j], injuryData);
                            }
                            break;
                        }
                    case FactionConsequenceTargetId.HIGHEST_RANKED_HERO:
                        {
                            for (int i = 0; i < HIGHEST_RANK_UNIT_TYPES.Length; i++)
                            {
                                if (unit != null)
                                {
                                    break;
                                }
                                unit = ctrlr.GetHighestRankUnit(HIGHEST_RANK_UNIT_TYPES[i], injuryData);
                            }
                            break;
                        }
                }
                if (unit != null)
                {
                    List<Item> list = new List<Item>();
                    bool flag = unit.AddInjury(injuryData, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, list, isHireUnit: false, factionConsequenceData.TreatmentTime);
                    PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list);
                    if (factionConsequenceData.TreatmentTime > 0)
                    {
                        unit.UnitSave.injuredTime = factionConsequenceData.TreatmentTime;
                    }
                    AddInjuryTime(unit, injuryData.Id);
                    if (!flag)
                    {
                        if (warSave.lateShipmentCount > 2)
                        {
                            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(unit, EventLogger.LogEvent.DEATH, (int)injuryData.Id);
                        }
                        else
                        {
                            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(unit, EventLogger.LogEvent.RETIREMENT, (int)injuryData.Id);
                        }
                    }
                }
            }
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.UpdateAttributes();
            primaryFactionCtrlr.CreateNewShipmentRequest(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate);
            if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
            {
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.GAME_OVER);
            }
        }
        return unit;
    }

    private void CheckForInjury(Unit unit)
    {
        Tuple<int, EventLogger.LogEvent, int> tuple = unit.Logger.FindLastEvent(EventLogger.LogEvent.NO_TREATMENT);
        Tuple<int, EventLogger.LogEvent, int> tuple2 = unit.Logger.FindLastEvent(EventLogger.LogEvent.RECOVERY);
        Tuple<int, EventLogger.LogEvent, int> tuple3 = null;
        if (tuple == null && tuple2 != null)
        {
            tuple3 = tuple2;
        }
        else if (tuple != null && tuple2 == null)
        {
            tuple3 = tuple;
        }
        else if (tuple != null && tuple2 != null)
        {
            tuple3 = ((tuple.Item1 <= tuple2.Item1) ? tuple2 : tuple);
        }
        if (tuple3 == null)
        {
            return;
        }
        if (tuple3.Item2 == EventLogger.LogEvent.NO_TREATMENT)
        {
            if (tuple3.Item1 != PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
            {
                return;
            }
            int @int = Constant.GetInt(ConstantId.INJURY_ROLL_INTERVAL);
            int num = tuple3.Item1 - unit.UnitSave.lastInjuryDate - 2;
            int num2 = Constant.GetInt(ConstantId.INJURY_ROLL_TARGET) + num / @int * Constant.GetInt(ConstantId.INJURY_DETERIORATION_PENALTY);
            if (num2 <= 1)
            {
                PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(unit, EventLogger.LogEvent.DEATH, tuple3.Item3);
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.TREATMENT_NOT_PAID);
                return;
            }
            int num3 = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 11);
            if (num3 >= num2)
            {
                PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(unit, EventLogger.LogEvent.DEATH, tuple3.Item3);
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.TREATMENT_NOT_PAID);
                PandoraDebug.LogInfo("unit dies from injury " + num3 + " >= " + num2, "MENUS");
            }
            else
            {
                unit.Logger.AddHistory(PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + @int, EventLogger.LogEvent.NO_TREATMENT, tuple3.Item3);
            }
        }
        else if (tuple3.Item2 == EventLogger.LogEvent.RECOVERY)
        {
            unit.UpdateInjury();
        }
    }

    private void CheckUpkeep(Unit unit)
    {
        if (unit.UnitSave.injuredTime > 0 && !unit.UnitSave.injuryPaid)
        {
            return;
        }
        int upkeepOwned = unit.GetUpkeepOwned();
        if (upkeepOwned != 0)
        {
            int num = (int)((float)upkeepOwned * Constant.GetFloat(ConstantId.UPKEEP_MISSED_DAY_PCT));
            int num2 = unit.AddToUpkeepOwned((num <= 0) ? 1 : num);
            if (num2 >= Constant.GetInt(ConstantId.UPKEEP_DAYS_WITHOUT_PAY))
            {
                unit.Logger.RemoveLastHistory(EventLogger.LogEvent.LEFT);
                PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(unit, EventLogger.LogEvent.LEFT, 0);
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.UPKEEP_NOT_PAID);
            }
        }
    }
}
