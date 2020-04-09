using Mono.Data.Sqlite;

public class EnchantmentConsumeData : DataCore
{
	public EnchantmentConsumeId Id
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
		Id = (EnchantmentConsumeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
