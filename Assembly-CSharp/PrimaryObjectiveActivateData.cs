using Mono.Data.Sqlite;

public class PrimaryObjectiveActivateData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public PrimaryObjectiveId PrimaryObjectiveId
	{
		get;
		private set;
	}

	public string PropsName
	{
		get;
		private set;
	}

	public int PropsCount
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(1);
		PropsName = reader.GetString(2);
		PropsCount = reader.GetInt32(3);
	}
}
