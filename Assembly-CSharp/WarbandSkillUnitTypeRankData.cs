using Mono.Data.Sqlite;

public class WarbandSkillUnitTypeRankData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandSkillId WarbandSkillId
	{
		get;
		private set;
	}

	public UnitTypeId UnitTypeId
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandSkillId = (WarbandSkillId)reader.GetInt32(1);
		UnitTypeId = (UnitTypeId)reader.GetInt32(2);
		Rank = reader.GetInt32(3);
	}
}
