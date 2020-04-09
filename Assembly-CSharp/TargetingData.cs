using Mono.Data.Sqlite;

public class TargetingData : DataCore
{
	public TargetingId Id
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
		Id = (TargetingId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
