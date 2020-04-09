using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTypeJoinBuildingData : DataCore
{
	public BuildingTypeId BuildingTypeId
	{
		get;
		private set;
	}

	public BuildingId BuildingId
	{
		get;
		private set;
	}

	public int Ratio
	{
		get;
		private set;
	}

	public bool Flippable
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		BuildingTypeId = (BuildingTypeId)reader.GetInt32(0);
		BuildingId = (BuildingId)reader.GetInt32(1);
		Ratio = reader.GetInt32(2);
		Flippable = (reader.GetInt32(3) != 0);
	}

	public static BuildingTypeJoinBuildingData GetRandomRatio(List<BuildingTypeJoinBuildingData> datas, Tyche tyche, Dictionary<BuildingTypeId, int> modifiers = null)
	{
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < datas.Count; i++)
		{
			int num2 = datas[i].Ratio;
			if (modifiers != null && modifiers.ContainsKey(datas[i].BuildingTypeId))
			{
				num2 = Mathf.Clamp(num2 + modifiers[datas[i].BuildingTypeId], 0, int.MaxValue);
			}
			num += num2;
			list.Add(num);
		}
		int num3 = tyche.Rand(0, num);
		for (int j = 0; j < list.Count; j++)
		{
			if (num3 < list[j])
			{
				return datas[j];
			}
		}
		return null;
	}
}
