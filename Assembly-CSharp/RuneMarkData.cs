using Mono.Data.Sqlite;

public class RuneMarkData : DataCore
{
	public RuneMarkId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public bool Rune
	{
		get;
		private set;
	}

	public bool Mark
	{
		get;
		private set;
	}

	public int Cost
	{
		get;
		private set;
	}

	public bool Released
	{
		get;
		private set;
	}

	public bool Lootable
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (RuneMarkId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Rune = (reader.GetInt32(2) != 0);
		Mark = (reader.GetInt32(3) != 0);
		Cost = reader.GetInt32(4);
		Released = (reader.GetInt32(5) != 0);
		Lootable = (reader.GetInt32(6) != 0);
	}
}
