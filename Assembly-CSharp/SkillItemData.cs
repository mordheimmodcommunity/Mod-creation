using Mono.Data.Sqlite;

public class SkillItemData : DataCore
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

	public ItemId ItemId
	{
		get;
		private set;
	}

	public MutationId MutationId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
		ItemId = (ItemId)reader.GetInt32(2);
		MutationId = (MutationId)reader.GetInt32(3);
	}
}
