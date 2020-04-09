using Mono.Data.Sqlite;

public class UnitJoinSkillData : DataCore
{
	public UnitId UnitId
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
		UnitId = (UnitId)reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
	}
}
