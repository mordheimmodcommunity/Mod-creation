using Mono.Data.Sqlite;

public class EnchantmentTypeData : DataCore
{
	public EnchantmentTypeId Id
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
		Id = (EnchantmentTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
