using Mono.Data.Sqlite;

public class WarbandNameData : DataCore
{
	public WarbandNameId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WarbandNameId)reader.GetInt32(0);
		Name = reader.GetString(1);
		WarbandId = (WarbandId)reader.GetInt32(2);
	}
}
