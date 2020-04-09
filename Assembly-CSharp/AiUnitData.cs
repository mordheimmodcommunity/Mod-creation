using Mono.Data.Sqlite;

public class AiUnitData : DataCore
{
	public AiUnitId Id
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
		Id = (AiUnitId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
