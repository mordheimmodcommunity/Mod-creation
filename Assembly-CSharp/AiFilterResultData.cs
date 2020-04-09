using Mono.Data.Sqlite;

public class AiFilterResultData : DataCore
{
	public AiFilterResultId Id
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
		Id = (AiFilterResultId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
