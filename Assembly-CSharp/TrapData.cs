using Mono.Data.Sqlite;

public class TrapData : DataCore
{
	public TrapId Id
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
		Id = (TrapId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
