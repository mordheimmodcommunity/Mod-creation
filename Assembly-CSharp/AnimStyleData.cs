using Mono.Data.Sqlite;

public class AnimStyleData : DataCore
{
	public AnimStyleId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string Size
	{
		get;
		private set;
	}

	public string Layer
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (AnimStyleId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Size = reader.GetString(2);
		Layer = reader.GetString(3);
	}
}
