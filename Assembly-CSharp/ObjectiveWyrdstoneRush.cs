using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveWyrdstoneRush : Objective
{
	private int wyrdstoneRushTarget;

	private PrimaryObjectiveWyrdData rushData;

	public ObjectiveWyrdstoneRush(PrimaryObjectiveId id, WarbandController warCtrlr, int seed)
		: base(id)
	{
		SetBaseData(seed);
		wyrdstoneRushTarget = (int)Mathf.Ceil((float)PandoraSingleton<MissionManager>.Instance.numWyrdstones * (float)rushData.WyrdstonePerc / 100f);
		searchToCheck.Add(warCtrlr.wagon.chest);
		unitsToCheck = warCtrlr.unitCtrlrs;
		counter.y = wyrdstoneRushTarget;
	}

	public ObjectiveWyrdstoneRush(PrimaryObjectiveId id, int seed)
		: base(id)
	{
		SetBaseData(seed);
	}

	protected override void Track(ref bool objectivesChanged)
	{
		int num = 0;
		for (int i = 0; i < searchToCheck.Count; i++)
		{
			num += CountWyrdstones(searchToCheck[i].items);
		}
		for (int j = 0; j < unitsToCheck.Count; j++)
		{
			if (unitsToCheck[j].unit.Status != UnitStateId.OUT_OF_ACTION)
			{
				num += CountWyrdstones(unitsToCheck[j].unit.Items);
			}
		}
		counter.x = num;
	}

	private int CountWyrdstones(List<Item> items)
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Id == ItemId.WYRDSTONE_SHARD || items[i].Id == ItemId.WYRDSTONE_FRAGMENT || items[i].Id == ItemId.WYRDSTONE_CLUSTER)
			{
				num++;
			}
		}
		return num;
	}

	private void SetBaseData(int seed)
	{
		List<PrimaryObjectiveWyrdData> datas = PandoraSingleton<DataFactory>.Instance.InitData<PrimaryObjectiveWyrdData>();
		Tyche tyche = new Tyche(seed);
		rushData = PrimaryObjectiveWyrdData.GetRandomRatio(datas, tyche);
		desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_obj_wyrdstone_rush_desc", rushData.WyrdstonePerc.ToString());
	}

	public override void Reload(uint trackedUid)
	{
		throw new NotImplementedException();
	}
}
