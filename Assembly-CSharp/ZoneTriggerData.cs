using Mono.Data.Sqlite;

public class ZoneTriggerData : DataCore
{
	public ZoneTriggerId Id
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
		Id = (ZoneTriggerId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
