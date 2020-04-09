using Mono.Data.Sqlite;

public class RuneMarkQualityData : DataCore
{
	public RuneMarkQualityId Id
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
		Id = (RuneMarkQualityId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
