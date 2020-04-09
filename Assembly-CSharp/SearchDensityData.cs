using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class SearchDensityData : DataCore
{
	public SearchDensityId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int SpawnMin
	{
		get;
		private set;
	}

	public int SpawnMax
	{
		get;
		private set;
	}

	public int Ratio
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (SearchDensityId)reader.GetInt32(0);
		Name = reader.GetString(1);
		SpawnMin = reader.GetInt32(2);
		SpawnMax = reader.GetInt32(3);
		Ratio = reader.GetInt32(4);
	}

	public static SearchDensityData GetRandomRatio(List<SearchDensityData> datas, Tyche tyche, Dictionary<SearchDensityId, int> modifiers = null)
	{
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < datas.Count; i++)
		{
			int num2 = datas[i].Ratio;
			if (modifiers != null && modifiers.ContainsKey(datas[i].Id))
			{
				num2 = Mathf.Clamp(num2 + modifiers[datas[i].Id], 0, int.MaxValue);
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
