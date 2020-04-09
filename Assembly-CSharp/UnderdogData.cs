using Mono.Data.Sqlite;

public class UnderdogData : DataCore
{
	public int Diff
	{
		get;
		private set;
	}

	public int XpBonus
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Diff = reader.GetInt32(0);
		XpBonus = reader.GetInt32(1);
	}
}
