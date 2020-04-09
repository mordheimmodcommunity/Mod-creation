using Mono.Data.Sqlite;

public class SearchZoneData : DataCore
{
	public SearchZoneId Id
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
		Id = (SearchZoneId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
