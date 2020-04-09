using Mono.Data.Sqlite;

public class RuneMarkEnchantmentData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public RuneMarkId RuneMarkId
	{
		get;
		private set;
	}

	public RuneMarkQualityId RuneMarkQualityId
	{
		get;
		private set;
	}

	public ItemTypeId ItemTypeId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		RuneMarkId = (RuneMarkId)reader.GetInt32(1);
		RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(2);
		ItemTypeId = (ItemTypeId)reader.GetInt32(3);
		EnchantmentId = (EnchantmentId)reader.GetInt32(4);
	}
}
