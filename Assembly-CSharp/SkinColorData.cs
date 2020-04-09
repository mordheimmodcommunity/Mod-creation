using Mono.Data.Sqlite;

public class SkinColorData : DataCore
{
	public SkinColorId Id
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
		Id = (SkinColorId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
