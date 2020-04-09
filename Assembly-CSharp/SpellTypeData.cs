using Mono.Data.Sqlite;

public class SpellTypeData : DataCore
{
	public SpellTypeId Id
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
		Id = (SpellTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
