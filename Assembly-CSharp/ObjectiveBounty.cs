using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveBounty : Objective
{
    private List<Unit> targets;

    private PrimaryObjectiveBountyData bountyData;

    private int targetsCount;

    public ObjectiveBounty(PrimaryObjectiveId id, WarbandController warCtrlr, int seed, List<Unit> enemies)
        : base(id)
    {
        Tyche tyche = new Tyche(seed);
        SetBaseData(enemies.Count, tyche);
        List<Unit> list = new List<Unit>();
        List<Unit> list2 = new List<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (enemy.GetUnitTypeId() == UnitTypeId.HENCHMEN)
            {
                list2.Add(enemy);
            }
            else
            {
                list.Add(enemy);
            }
        }
        targets = new List<Unit>();
        for (int i = 0; i < targetsCount; i++)
        {
            List<Unit> list3 = (i >= bountyData.EliteCount || list.Count <= 0) ? list2 : list;
            if (list3.Count == 0)
            {
                targetsCount--;
                continue;
            }
            int index = tyche.Rand(0, list3.Count);
            itemsToSteal.Add(list3[index].deathTrophy);
            AddSubObj(list3[index].Name, list3[index].deathTrophy.Name);
            targets.Add(list3[index]);
            list3.RemoveAt(index);
        }
        searchToCheck.Add(warCtrlr.wagon.chest);
        unitsToCheck = warCtrlr.unitCtrlrs;
        counter.y = targetsCount;
    }

    public ObjectiveBounty(PrimaryObjectiveId id, List<UnitSave> enemies, int seed)
        : base(id)
    {
        Tyche tyche = new Tyche(seed);
        SetBaseData(enemies.Count, tyche);
        List<UnitSave> list = new List<UnitSave>();
        List<UnitSave> list2 = new List<UnitSave>();
        foreach (UnitSave enemy in enemies)
        {
            UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(enemy.stats.id);
            UnitTypeId unitTypeId = Unit.GetUnitTypeId(enemy, unitData.UnitTypeId);
            if (unitTypeId == UnitTypeId.HENCHMEN)
            {
                list2.Add(enemy);
            }
            else
            {
                list.Add(enemy);
            }
        }
        for (int i = 0; i < targetsCount; i++)
        {
            List<UnitSave> list3 = (i >= bountyData.EliteCount || list.Count <= 0) ? list2 : list;
            if (list3.Count == 0)
            {
                targetsCount--;
                continue;
            }
            int index = tyche.Rand(0, list3.Count);
            UnitData unitData2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(list3[index].stats.id);
            AddSubObj(list3[index].stats.Name, unitData2.ItemIdTrophy.ToLowerString());
            list3.RemoveAt(index);
        }
    }

    private void SetBaseData(int totalCount, Tyche tyche)
    {
        List<PrimaryObjectiveBountyData> datas = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveBountyData>();
        bountyData = PrimaryObjectiveBountyData.GetRandomRatio(datas, tyche);
        targetsCount = Mathf.RoundToInt((float)totalCount * (float)bountyData.UnitPerc / 100f);
        targetsCount = Mathf.Clamp(targetsCount, 1, totalCount);
        desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_obj_bounty_desc");
    }

    protected override void Track(ref bool objectivesChanged)
    {
        CheckItemsToSteal(ref objectivesChanged);
    }

    private void AddSubObj(string unitName, string itemName)
    {
        subDesc.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_name_" + itemName, unitName));
        dones.Add(item: false);
    }

    public bool IsUnitBounty(UnitController unit)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == unit.unit)
            {
                return true;
            }
        }
        return false;
    }

    public override void Reload(uint trackedUid)
    {
        throw new NotImplementedException();
    }
}
