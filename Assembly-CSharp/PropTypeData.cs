using Mono.Data.Sqlite;

public class PropTypeData : DataCore
{
	public PropTypeId Id
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
		Id = (PropTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
