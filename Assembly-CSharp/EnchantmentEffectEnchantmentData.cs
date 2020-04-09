using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class EnchantmentEffectEnchantmentData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentIdEffect
	{
		get;
		private set;
	}

	public EnchantmentTriggerId EnchantmentTriggerId
	{
		get;
		private set;
	}

	public AttributeId AttributeIdRoll
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
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
		EnchantmentIdEffect = (EnchantmentId)reader.GetInt32(2);
		EnchantmentTriggerId = (EnchantmentTriggerId)reader.GetInt32(3);
		AttributeIdRoll = (AttributeId)reader.GetInt32(4);
		UnitActionIdTrigger = (UnitActionId)reader.GetInt32(5);
		SkillIdTrigger = (SkillId)reader.GetInt32(6);
		Ratio = reader.GetInt32(7);
		Self = (reader.GetInt32(8) != 0);
		TargetSelf = (reader.GetInt32(9) != 0);
		TargetAlly = (reader.GetInt32(10) != 0);
		TargetEnemy = (reader.GetInt32(11) != 0);
	}

	public static EnchantmentEffectEnchantmentData GetRandomRatio(List<EnchantmentEffectEnchantmentData> datas, Tyche tyche, Dictionary<int, int> modifiers = null)
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
