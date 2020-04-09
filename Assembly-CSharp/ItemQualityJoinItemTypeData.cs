using Mono.Data.Sqlite;

public class ItemQualityJoinItemTypeData : DataCore
{
	public ItemQualityId ItemQualityId
	{
		get;
		private set;
	}

	public ItemTypeId ItemTypeId
	{
		get;
		private set;
	}

	public int RatingModifier
	{
		get;
		private set;
	}

	public int DamageMinModifier
	{
		get;
		private set;
	}

	public int DamageMaxModifier
	{
		get;
		private set;
	}

	public int ArmorAbsorptionModifier
	{
		get;
		private set;
	}

	public int RangeModifier
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		ItemQualityId = (ItemQualityId)reader.GetInt32(0);
		ItemTypeId = (ItemTypeId)reader.GetInt32(1);
		RatingModifier = reader.GetInt32(2);
		DamageMinModifier = reader.GetInt32(3);
		DamageMaxModifier = reader.GetInt32(4);
		ArmorAbsorptionModifier = reader.GetInt32(5);
		RangeModifier = reader.GetInt32(6);
	}
}
