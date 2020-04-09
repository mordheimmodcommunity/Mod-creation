using Mono.Data.Sqlite;

public class FactionRankWarbandSkillData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public FactionId FactionId
	{
		get;
		private set;
	}

	public FactionRankId FactionRankId
	{
		get;
		private set;
	}

	public WarbandSkillId WarbandSkillId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		FactionId = (FactionId)reader.GetInt32(1);
		FactionRankId = (FactionRankId)reader.GetInt32(2);
		WarbandSkillId = (WarbandSkillId)reader.GetInt32(3);
	}
}
