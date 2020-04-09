using Mono.Data.Sqlite;

public class WarbandSkillTypeData : DataCore
{
	public WarbandSkillTypeId Id
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
		Id = (WarbandSkillTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
