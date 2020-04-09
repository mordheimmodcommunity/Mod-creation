using Mono.Data.Sqlite;

public class PlayerTypeData : DataCore
{
	public PlayerTypeId Id
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
		Id = (PlayerTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
