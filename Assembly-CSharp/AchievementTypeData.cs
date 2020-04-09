using Mono.Data.Sqlite;

public class AchievementTypeData : DataCore
{
	public AchievementTypeId Id
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
		Id = (AchievementTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
