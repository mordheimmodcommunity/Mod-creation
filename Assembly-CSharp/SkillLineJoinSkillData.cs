using Mono.Data.Sqlite;

public class SkillLineJoinSkillData : DataCore
{
	public SkillLineId SkillLineId
	{
		get;
		private set;
	}

	public SkillId SkillId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		SkillLineId = (SkillLineId)reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
	}
}
