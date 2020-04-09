using Mono.Data.Sqlite;

public class EnchantmentQualityData : DataCore
{
	public EnchantmentQualityId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (EnchantmentQualityId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
