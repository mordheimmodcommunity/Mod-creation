using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class SkillEnchantmentData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public SkillId SkillId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public EnchantmentTriggerId EnchantmentTriggerId
	{
		get;
		private set;
	}

	public UnitActionId UnitActionIdTrigger
	{
		get;
		private set;
	}

	public SkillId SkillIdTrigger
	{
		get;
		private set;
	}

	public int Ratio
	{
		get;
		private set;
	}

	public bool Self
	{
		get;
		private set;
	}

	public bool TargetSelf
	{
		get;
		private set;
	}

	public bool TargetAlly
	{
		get;
		private set;
	}

	public bool TargetEnemy
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
		EnchantmentId = (EnchantmentId)reader.GetInt32(2);
		EnchantmentTriggerId = (EnchantmentTriggerId)reader.GetInt32(3);
		UnitActionIdTrigger = (UnitActionId)reader.GetInt32(4);
		SkillIdTrigger = (SkillId)reader.GetInt32(5);
		Ratio = reader.GetInt32(6);
		Self = (reader.GetInt32(7) != 0);
		TargetSelf = (reader.GetInt32(8) != 0);
		TargetAlly = (reader.GetInt32(9) != 0);
		TargetEnemy = (reader.GetInt32(10) != 0);
	}

	public static SkillEnchantmentData GetRandomRatio(List<SkillEnchantmentData> datas, Tyche tyche, Dictionary<int, int> modifiers = null)
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
