using Mono.Data.Sqlite;

public class TableFilterData : DataCore
{
	public TableFilterId Id
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
		Id = (TableFilterId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
