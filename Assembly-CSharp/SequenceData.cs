using Mono.Data.Sqlite;

public class SequenceData : DataCore
{
	public SequenceId Id
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
		Id = (SequenceId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
