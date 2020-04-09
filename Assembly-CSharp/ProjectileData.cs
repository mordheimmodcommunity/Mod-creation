using Mono.Data.Sqlite;

public class ProjectileData : DataCore
{
	public ProjectileId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string Sound
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ProjectileId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Sound = reader.GetString(2);
	}
}
