using Mono.Data.Sqlite;

public class ItemSpeedData : DataCore
{
	public ItemSpeedId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Speed
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ItemSpeedId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Speed = reader.GetInt32(2);
	}
}
