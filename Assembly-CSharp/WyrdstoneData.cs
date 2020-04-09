using Mono.Data.Sqlite;

public class WyrdstoneData : DataCore
{
	public WyrdstoneId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public bool Outdoor
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WyrdstoneId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Outdoor = (reader.GetInt32(2) != 0);
	}
}
