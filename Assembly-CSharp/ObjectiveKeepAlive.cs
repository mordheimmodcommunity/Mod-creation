using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveKeepAlive : Objective
{
    private Unit keepAliveUnit;

    public ObjectiveKeepAlive(PrimaryObjectiveId id)
        : base(id)
    {
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)id;
        PrimaryObjectiveKeepAliveData primaryObjectiveKeepAliveData = instance.InitData<PrimaryObjectiveKeepAliveData>("fk_primary_objective_id", num.ToString())[0];
        List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
        for (int i = 0; i < allUnits.Count; i++)
        {
            if (allUnits[i].unit.CampaignData != null && allUnits[i].unit.CampaignData.Id == primaryObjectiveKeepAliveData.CampaignUnitId)
            {
                keepAliveUnit = allUnits[i].unit;
                break;
            }
        }
        counter = new Vector2(1f, 1f);
    }

    public override void Reload(uint trackedUid)
    {
        throw new NotImplementedException();
    }

    protected override void Track(ref bool objectivesChanged)
    {
        counter.x = ((keepAliveUnit.Status != UnitStateId.OUT_OF_ACTION) ? 1 : 0);
    }
}
