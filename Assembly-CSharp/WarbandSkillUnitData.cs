using Mono.Data.Sqlite;

public class WarbandSkillUnitData : DataCore
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

	public UnitId UnitId
	{
		get;
		private set;
	}

	public bool BaseUnit
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandSkillId = (WarbandSkillId)reader.GetInt32(1);
		UnitId = (UnitId)reader.GetInt32(2);
		BaseUnit = (reader.GetInt32(3) != 0);
	}
}
