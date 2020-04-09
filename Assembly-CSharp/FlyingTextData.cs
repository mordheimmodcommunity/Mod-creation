using Mono.Data.Sqlite;

public class FlyingTextData : DataCore
{
	public FlyingTextId Id
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
		Id = (FlyingTextId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
