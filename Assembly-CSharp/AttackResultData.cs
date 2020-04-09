using Mono.Data.Sqlite;

public class AttackResultData : DataCore
{
	public AttackResultId Id
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
		Id = (AttackResultId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
