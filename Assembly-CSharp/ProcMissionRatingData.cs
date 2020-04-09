using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class ProcMissionRatingData : DataCore
{
	public ProcMissionRatingId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Ratio
	{
		get;
		private set;
	}

	public int MinValue
	{
		get;
		private set;
	}

	public int MaxValue
	{
		get;
		private set;
	}

	public int ProcMinValue
	{
		get;
		private set;
	}

	public int ProcMaxValue
	{
		get;
		private set;
	}

	public int RewardSearchPerc
	{
		get;
		private set;
	}

	public int RewardWyrdstonePerc
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public int UnderdogXp
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ProcMissionRatingId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Ratio = reader.GetInt32(2);
		MinValue = reader.GetInt32(3);
		MaxValue = reader.GetInt32(4);
		ProcMinValue = reader.GetInt32(5);
		ProcMaxValue = reader.GetInt32(6);
		RewardSearchPerc = reader.GetInt32(7);
		RewardWyrdstonePerc = reader.GetInt32(8);
		EnchantmentId = (EnchantmentId)reader.GetInt32(9);
		UnderdogXp = reader.GetInt32(10);
	}

	public static ProcMissionRatingData GetRandomRatio(List<ProcMissionRatingData> datas, Tyche tyche, Dictionary<ProcMissionRatingId, int> modifiers = null)
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
