using Mono.Data.Sqlite;

public class UnitActionRefreshData : DataCore
{
	public UnitActionRefreshId Id
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
		Id = (UnitActionRefreshId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
