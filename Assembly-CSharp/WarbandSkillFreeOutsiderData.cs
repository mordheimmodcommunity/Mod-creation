using Mono.Data.Sqlite;

public class WarbandSkillFreeOutsiderData : DataCore
{
	public WarbandSkillFreeOutsiderId Id
	{
		get;
		private set;
	}

	public string Name
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

	public int Rank
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WarbandSkillFreeOutsiderId)reader.GetInt32(0);
		Name = reader.GetString(1);
		WarbandSkillId = (WarbandSkillId)reader.GetInt32(2);
		UnitId = (UnitId)reader.GetInt32(3);
		Rank = reader.GetInt32(4);
	}
}
