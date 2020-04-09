using System.Collections.Generic;
using UnityEngine;

public class SquadManager
{
	private const int MEMBERS_COUNT = 3;

	private const float MIN_MEMBERS_SQR_DIST = 900f;

	public const int SPOTTED_TURN_LIMIT = 2;

	public List<Squad> squads;

	private WarbandController warCtrlr;

	public List<KeyValuePair<UnitController, int>> spottedUnitsData = new List<KeyValuePair<UnitController, int>>();

	private List<UnitController> spottedEnemies = new List<UnitController>();

	public SquadManager(WarbandController warbandController)
	{
		warCtrlr = warbandController;
		squads = new List<Squad>();
	}

	public void FormSquads()
	{
		List<UnitController> list = new List<UnitController>(warCtrlr.unitCtrlrs);
		UnitController leader = warCtrlr.GetLeader();
		leader = ((!(leader == null)) ? leader : warCtrlr.unitCtrlrs[0]);
		FormSquad(leader, list);
		while (list.Count > 0)
		{
			FormSquad(UnitWithClosestAlly(list), list);
		}
	}

	private void FormSquad(UnitController squadLeader, List<UnitController> allUnits)
	{
		Squad squad = new Squad();
		int count = squads.Count;
		squad.members.Add(squadLeader);
		squadLeader.AICtrlr.SetSquad(squad, count);
		allUnits.Remove(squadLeader);
		for (int i = squad.members.Count; i < 3; i++)
		{
			if (allUnits.Count <= 0)
			{
				break;
			}
			int num = -1;
			float num2 = float.MaxValue;
			for (int j = 0; j < allUnits.Count; j++)
			{
				float num3 = Vector3.SqrMagnitude(squad.members[0].transform.position - allUnits[j].transform.position);
				if (num3 < num2)
				{
					num2 = num3;
					num = j;
				}
			}
			if (num != -1 && num2 < 900f)
			{
				squad.members.Add(allUnits[num]);
				allUnits[num].AICtrlr.SetSquad(squad, count);
				allUnits.RemoveAt(num);
			}
		}
		squads.Add(squad);
	}

	public void RefreshSquads()
	{
		for (int num = squads.Count - 1; num >= 0; num--)
		{
			squads[num].RemoveDeadMembers();
			if (squads[num].members.Count == 0)
			{
				squads.RemoveAt(num);
			}
		}
		List<UnitController> list = new List<UnitController>();
		for (int i = 0; i < squads.Count; i++)
		{
			if (squads[i].LoneLostLastMember())
			{
				list.Add(squads[i].members[0]);
				squads.RemoveAt(i);
			}
		}
		for (int j = 0; j < warCtrlr.unitCtrlrs.Count; j++)
		{
			if (warCtrlr.unitCtrlrs[j].AICtrlr.Squad == null)
			{
				list.Add(warCtrlr.unitCtrlrs[j]);
			}
		}
		if (squads.Count == 0)
		{
			squads.Add(new Squad());
		}
		for (int k = 0; k < list.Count; k++)
		{
			Squad squad = squads[0];
			float num2 = float.MaxValue;
			for (int l = 0; l < squads.Count; l++)
			{
				for (int m = 0; m < squads[l].members.Count; m++)
				{
					float num3 = Vector3.SqrMagnitude(squads[l].members[m].transform.position - list[k].transform.position);
					if (num3 < num2)
					{
						num2 = num3;
						squad = squads[l];
					}
				}
			}
			list[k].AICtrlr.SetSquad(squad, squads.IndexOf(squad));
			squad.members.Add(list[k]);
		}
	}

	private UnitController UnitWithClosestAlly(List<UnitController> allUnits)
	{
		UnitController result = allUnits[0];
		float num = float.MaxValue;
		for (int i = 0; i < allUnits.Count - 1; i++)
		{
			for (int j = i + 1; j < allUnits.Count; j++)
			{
				float num2 = Vector3.SqrMagnitude(allUnits[i].transform.position - allUnits[j].transform.position);
				if (num2 < num)
				{
					num = num2;
					result = allUnits[i];
				}
			}
		}
		return result;
	}

	public void UnitSpotted(UnitController ctrlr)
	{
		for (int i = 0; i < spottedUnitsData.Count; i++)
		{
			if (spottedUnitsData[i].Key == ctrlr)
			{
				spottedUnitsData[i] = new KeyValuePair<UnitController, int>(ctrlr, PandoraSingleton<MissionManager>.Instance.currentTurn);
				return;
			}
		}
		spottedUnitsData.Add(new KeyValuePair<UnitController, int>(ctrlr, PandoraSingleton<MissionManager>.Instance.currentTurn));
	}

	public List<UnitController> GetSpottedEnemies()
	{
		spottedEnemies.Clear();
		for (int i = 0; i < spottedUnitsData.Count; i++)
		{
			if (spottedUnitsData[i].Key.unit.Status != UnitStateId.OUT_OF_ACTION)
			{
				spottedEnemies.Add(spottedUnitsData[i].Key);
			}
		}
		return spottedEnemies;
	}
}
