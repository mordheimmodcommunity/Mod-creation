using Mono.Data.Sqlite;

public class ZoneAoeData : DataCore
{
	public ZoneAoeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Duration
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ZoneAoeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Duration = reader.GetInt32(2);
	}
}
