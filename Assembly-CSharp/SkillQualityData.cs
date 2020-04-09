using Mono.Data.Sqlite;

public class SkillQualityData : DataCore
{
	public SkillQualityId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (SkillQualityId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Rating = reader.GetInt32(2);
	}
}
