using Mono.Data.Sqlite;

public class AllegianceData : DataCore
{
	public AllegianceId Id
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
		Id = (AllegianceId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
