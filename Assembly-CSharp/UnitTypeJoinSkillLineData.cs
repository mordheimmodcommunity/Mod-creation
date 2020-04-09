using Mono.Data.Sqlite;

public class UnitTypeJoinSkillLineData : DataCore
{
	public UnitTypeId UnitTypeId
	{
		get;
		private set;
	}

	public SkillLineId SkillLineId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		UnitTypeId = (UnitTypeId)reader.GetInt32(0);
		SkillLineId = (SkillLineId)reader.GetInt32(1);
	}
}
