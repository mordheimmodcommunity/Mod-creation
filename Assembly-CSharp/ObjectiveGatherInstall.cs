using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveGatherInstall : Objective
{
	private List<PrimaryObjectiveGatherInstallData> gathersData;

	public ObjectiveGatherInstall(PrimaryObjectiveId id, WarbandController warCtrlr)
		: base(id)
	{
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		int num = (int)id;
		gathersData = instance.InitData<PrimaryObjectiveGatherInstallData>("fk_primary_objective_id", num.ToString());
		List<Item> list = new List<Item>();
		searchToCheck = new List<SearchPoint>();
		for (int i = 0; i < gathersData.Count; i++)
		{
			if (gathersData[i].ItemId != 0)
			{
				list = PandoraSingleton<MissionManager>.Instance.FindObjectivesInSearch(gathersData[i].ItemId);
				PandoraSingleton<MissionManager>.Instance.FindObjectiveInUnits(gathersData[i].ItemId, ref list);
				itemsToSteal.AddRange(list);
			}
			if (!string.IsNullOrEmpty(gathersData[i].CheckSearch))
			{
				searchToCheck.AddRange(PandoraSingleton<MissionManager>.Instance.GetSearchPoints(gathersData[i].CheckSearch));
			}
			if (gathersData[i].CheckWagon)
			{
				searchToCheck.Add(warCtrlr.wagon.chest);
			}
			if (gathersData[i].CheckUnits)
			{
				unitsToCheck = warCtrlr.unitCtrlrs;
			}
		}
		counter = new Vector2(0f, gathersData[0].ItemCount);
		desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, gathersData[0].ItemId.ToLowerString());
	}

	public override void Reload(uint trackedUid)
	{
		throw new NotImplementedException();
	}

	protected override void Track(ref bool objectivesChanged)
	{
		CheckItemsToSteal(ref objectivesChanged);
	}
}
