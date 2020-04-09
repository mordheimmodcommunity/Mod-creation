using Mono.Data.Sqlite;

public class PrimaryObjectiveRequirementData : DataCore
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

	public PrimaryObjectiveId PrimaryObjectiveIdRequired
	{
		get;
		private set;
	}

	public bool RequiredCompleted
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(1);
		PrimaryObjectiveIdRequired = (PrimaryObjectiveId)reader.GetInt32(2);
		RequiredCompleted = (reader.GetInt32(3) != 0);
	}
}
