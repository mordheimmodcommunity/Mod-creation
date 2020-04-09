using Mono.Data.Sqlite;

public class AiProfileData : DataCore
{
	public AiProfileId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public AiUnitId AiUnitIdBase
	{
		get;
		private set;
	}

	public AiUnitId AiUnitIdAlternate
	{
		get;
		private set;
	}

	public AiUnitId AiUnitIdSearch
	{
		get;
		private set;
	}

	public AiUnitId AiUnitIdSkillSpellTarget
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (AiProfileId)reader.GetInt32(0);
		Name = reader.GetString(1);
		AiUnitIdBase = (AiUnitId)reader.GetInt32(2);
		AiUnitIdAlternate = (AiUnitId)reader.GetInt32(3);
		AiUnitIdSearch = (AiUnitId)reader.GetInt32(4);
		AiUnitIdSkillSpellTarget = (AiUnitId)reader.GetInt32(5);
	}
}
