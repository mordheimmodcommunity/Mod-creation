using Mono.Data.Sqlite;

public class SkillPerformSkillData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public SkillId SkillId
	{
		get;
		private set;
	}

	public SkillId SkillIdPerformed
	{
		get;
		private set;
	}

	public AttributeId AttributeIdRoll
	{
		get;
		private set;
	}

	public bool Success
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
		SkillIdPerformed = (SkillId)reader.GetInt32(2);
		AttributeIdRoll = (AttributeId)reader.GetInt32(3);
		Success = (reader.GetInt32(4) != 0);
	}
}
