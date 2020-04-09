using Mono.Data.Sqlite;

public class InjuryRepeatData : DataCore
{
	public InjuryRepeatId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public InjuryRepeatId InjuryRepeatIdResult
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (InjuryRepeatId)reader.GetInt32(0);
		Name = reader.GetString(1);
		InjuryRepeatIdResult = (InjuryRepeatId)reader.GetInt32(2);
	}
}
