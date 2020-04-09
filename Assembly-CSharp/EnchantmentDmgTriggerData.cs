using Mono.Data.Sqlite;

public class EnchantmentDmgTriggerData : DataCore
{
	public EnchantmentDmgTriggerId Id
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
		Id = (EnchantmentDmgTriggerId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
