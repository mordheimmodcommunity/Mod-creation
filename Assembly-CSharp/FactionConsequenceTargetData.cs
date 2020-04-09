using Mono.Data.Sqlite;

public class FactionConsequenceTargetData : DataCore
{
	public FactionConsequenceTargetId Id
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
		Id = (FactionConsequenceTargetId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
