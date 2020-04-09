using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class SearchDensityLootData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public SearchDensityId SearchDensityId
	{
		get;
		private set;
	}

	public SearchRewardId SearchRewardId
	{
		get;
		private set;
	}

	public int WarbandRank
	{
		get;
		private set;
	}

	public int ItemMin
	{
		get;
		private set;
	}

	public int ItemMax
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
		Id = reader.GetInt32(0);
		SearchDensityId = (SearchDensityId)reader.GetInt32(1);
		SearchRewardId = (SearchRewardId)reader.GetInt32(2);
		WarbandRank = reader.GetInt32(3);
		ItemMin = reader.GetInt32(4);
		ItemMax = reader.GetInt32(5);
		Ratio = reader.GetInt32(6);
	}

	public static SearchDensityLootData GetRandomRatio(List<SearchDensityLootData> datas, Tyche tyche, Dictionary<int, int> modifiers = null)
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
