using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveWanted : Objective
{
    private List<Unit> wantedUnits;

    public ObjectiveWanted(PrimaryObjectiveId id, WarbandController warCtrlr)
        : base(id)
    {
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)id;
        List<PrimaryObjectiveWantedData> list = instance.InitData<PrimaryObjectiveWantedData>("fk_primary_objective_id", num.ToString());
        List<UnitController> allEnemies = PandoraSingleton<MissionManager>.Instance.GetAllEnemies(warCtrlr.idx);
        wantedUnits = new List<Unit>();
        for (int i = 0; i < list.Count; i++)
        {
            Unit unit = null;
            for (int j = 0; j < allEnemies.Count; j++)
            {
                if (allEnemies[j].unit.UnitSave.campaignId == (int)list[i].CampaignUnitId)
                {
                    unit = allEnemies[j].unit;
                    wantedUnits.Add(unit);
                    if (list.Count > 1)
                    {
                        subDesc.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_sub_obj_wanted", allEnemies[j].unit.Name));
                    }
                    dones.Add(item: false);
                }
            }
        }
        counter = new Vector2(0f, wantedUnits.Count);
    }

    public override void Reload(uint trackedUid)
    {
        throw new NotImplementedException();
    }

    protected override void Track(ref bool objectivesChanged)
    {
        counter.x = 0f;
        for (int i = 0; i < wantedUnits.Count; i++)
        {
            dones[i] = (wantedUnits[i].Status == UnitStateId.OUT_OF_ACTION);
            counter.x += (dones[i] ? 1 : 0);
        }
    }
}
