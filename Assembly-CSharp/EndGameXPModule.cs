using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameXPModule : UIModule
{
    public ListGroup xpList;

    public ListGroup advList;

    public GameObject xpItem;

    public GameObject advItem;

    public void Set(MissionEndUnitSave endUnit, Unit unit)
    {
        xpList.gameObject.SetActive(value: true);
        advList.gameObject.SetActive(value: true);
        xpList.ClearList();
        advList.ClearList();
        if (endUnit.XPs.Count > 0)
        {
            PandoraDebug.LogDebug("Unit Name = " + endUnit.unitSave.stats.Name + "XP Count = " + endUnit.XPs.Count);
            if (unit.IsMaxRank())
            {
                xpList.Setup("hideout_max_rank", xpItem);
                showQueue.Enqueue(xpList.gameObject);
            }
            else
            {
                xpList.Setup("menu_experience", xpItem);
                showQueue.Enqueue(xpList.gameObject);
                for (int i = 0; i < endUnit.XPs.Count; i++)
                {
                    AddXp(string.Empty + endUnit.XPs[i].Key, endUnit.XPs[i].Value);
                }
            }
        }
        bool flag = false;
        if (endUnit.advancements.Count > 0)
        {
            PandoraDebug.LogDebug("Unit Name = " + endUnit.unitSave.stats.Name + "ADV Count = " + endUnit.advancements.Count);
            advList.Setup("menu_advancement", advItem);
            showQueue.Enqueue(advList.gameObject);
            for (int j = 0; j < endUnit.advancements.Count; j++)
            {
                if (endUnit.advancements[j].Martial > 0)
                {
                    AddAdvancement("unit_adv_martial", endUnit.advancements[j].Martial.ToString());
                }
                if (endUnit.advancements[j].Mental > 0)
                {
                    AddAdvancement("unit_adv_mental", endUnit.advancements[j].Mental.ToString());
                }
                if (endUnit.advancements[j].Physical > 0)
                {
                    AddAdvancement("unit_adv_physical", endUnit.advancements[j].Physical.ToString());
                }
                if (endUnit.advancements[j].Mutation)
                {
                    if (!flag)
                    {
                        PandoraSingleton<Pan>.Instance.Narrate("mutation" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 6));
                        flag = true;
                    }
                    AddAdvancement("unit_adv_mutation", "1");
                }
                if (endUnit.advancements[j].Spell > 0)
                {
                    AddAdvancement("unit_adv_spell", endUnit.advancements[j].Spell.ToString());
                }
                if (endUnit.advancements[j].Skill > 0)
                {
                    AddAdvancement("unit_adv_skill", endUnit.advancements[j].Skill.ToString());
                }
                if (endUnit.advancements[j].Offense > 0)
                {
                    AddAdvancement("unit_adv_off", endUnit.advancements[j].Offense.ToString());
                }
                if (endUnit.advancements[j].Strategy > 0)
                {
                    AddAdvancement("unit_adv_strat", endUnit.advancements[j].Strategy.ToString());
                }
            }
        }
        xpList.gameObject.SetActive(value: false);
        advList.gameObject.SetActive(value: false);
        StartShow(0.5f);
    }

    public void Set(Warband warband)
    {
        VictoryTypeId victoryType = warband.GetWarbandSave().endMission.VictoryType;
        xpList.gameObject.SetActive(value: true);
        advList.gameObject.SetActive(value: true);
        xpList.ClearList();
        advList.ClearList();
        if (warband.rankGained || warband.Rank < Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            xpList.Setup("menu_warband_experience", xpItem);
            showQueue.Enqueue(xpList.gameObject);
            AddXp(PandoraSingleton<DataFactory>.Instance.InitData<VictoryTypeData>((int)victoryType).WarbandExperience.ToString(), "mission_victory_" + victoryType.ToLowerString());
        }
        if (warband.rankGained)
        {
            PandoraSingleton<Pan>.Instance.Narrate("new_warband_rank");
            warband.Logger.AddHistory(warband.GetWarbandSave().currentDate, EventLogger.LogEvent.RANK_ACHIEVED, warband.Rank);
            advList.Setup("menu_advancement", advItem);
            showQueue.Enqueue(advList.gameObject);
            int id = warband.Rank - 1;
            WarbandRankSlotData warbandRankSlotData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankSlotData>("fk_warband_rank_id", id.ToString())[0];
            WarbandRankSlotData warbandRankSlotData2 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankSlotData>("fk_warband_rank_id", warband.Rank.ToString())[0];
            int num = warbandRankSlotData2.Henchman - warbandRankSlotData.Henchman;
            if (num > 0)
            {
                string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + UnitTypeId.HENCHMEN.ToString());
                AddAdvancement("warband_adv_slot_gained", num.ToString(), stringById);
            }
            num = warbandRankSlotData2.Hero - warbandRankSlotData.Hero;
            if (num > 0)
            {
                string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + UnitTypeId.HERO_1.ToString());
                AddAdvancement("warband_adv_slot_gained", num.ToString(), stringById2);
            }
            num = warbandRankSlotData2.Impressive - warbandRankSlotData.Impressive;
            if (num > 0)
            {
                if (warbandRankSlotData.Impressive > 0)
                {
                    string stringById3 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + UnitTypeId.IMPRESSIVE.ToString());
                    AddAdvancement("warband_adv_slot_gained", "1", stringById3);
                }
                else
                {
                    string stringById4 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + UnitTypeId.HERO_1.ToString());
                    AddAdvancement("warband_adv_slot_gained", num.ToString(), stringById4);
                }
            }
            num = warbandRankSlotData2.Leader - warbandRankSlotData.Leader;
            if (num > 0)
            {
                string stringById5 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + UnitTypeId.LEADER.ToString());
                AddAdvancement("warband_adv_slot_gained", num.ToString(), stringById5);
            }
            num = warbandRankSlotData2.Reserve - warbandRankSlotData.Reserve;
            if (num > 0)
            {
                string stringById6 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_menu_warband_reserve");
                AddAdvancement("warband_adv_slot_gained", num.ToString(), stringById6);
            }
            List<WarbandRankJoinUnitTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankJoinUnitTypeData>("fk_warband_rank_id", warband.Rank.ToString());
            foreach (WarbandRankJoinUnitTypeData item in list)
            {
                if (item.UnitTypeId == UnitTypeId.IMPRESSIVE)
                {
                    AddAdvancement("warband_adv_impressive_unit_unlock");
                }
                else
                {
                    AddAdvancement("warband_adv_hero_unit_unlock");
                }
                PandoraSingleton<Pan>.Instance.Narrate("new_warriors");
            }
            WarbandRankData warbandRankData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(id);
            WarbandRankData warbandRankData2 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(warband.Rank);
            if (warbandRankData2.CartSize - warbandRankData.CartSize > 0)
            {
                AddAdvancement("warband_adv_cart", (warbandRankData2.CartSize - warbandRankData.CartSize).ToString());
            }
            AddAdvancement("hideout_warband_adv_idol_" + warband.Rank);
        }
        xpList.gameObject.SetActive(value: false);
        advList.gameObject.SetActive(value: false);
        StartShow(0.5f);
    }

    private void AddXp(string value, string title)
    {
        GameObject gameObject = xpList.AddToList();
        UIDescription component = gameObject.GetComponent<UIDescription>();
        gameObject.SetActive(value: false);
        component.title.set_text(value);
        component.desc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(title));
        showQueue.Enqueue(gameObject);
    }

    private void AddAdvancement(string locKey, params string[] parms)
    {
        GameObject gameObject = advList.AddToList();
        Text componentInChildren = gameObject.GetComponentInChildren<Text>();
        componentInChildren.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(locKey, parms));
        showQueue.Enqueue(gameObject);
        gameObject.SetActive(value: false);
    }
}
