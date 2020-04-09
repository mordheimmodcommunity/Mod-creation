using Mono.Data.Sqlite;

public class ItemConsumableLockConsumableData : DataCore
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

	public SkillId SkillIdLocked
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
		SkillIdLocked = (SkillId)reader.GetInt32(2);
	}
}
