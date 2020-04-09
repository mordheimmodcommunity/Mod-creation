using Mono.Data.Sqlite;

public class TrapTypeData : DataCore
{
	public TrapTypeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Perc
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (TrapTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Perc = reader.GetInt32(2);
	}
}
