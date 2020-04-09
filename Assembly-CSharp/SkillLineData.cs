using Mono.Data.Sqlite;

public class SkillLineData : DataCore
{
	public SkillLineId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public SkillLineId SkillLineIdDisplayed
	{
		get;
		private set;
	}

	public WarbandAttributeId WarbandAttributeIdPriceModifier
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (SkillLineId)reader.GetInt32(0);
		Name = reader.GetString(1);
		SkillLineIdDisplayed = (SkillLineId)reader.GetInt32(2);
		WarbandAttributeIdPriceModifier = (WarbandAttributeId)reader.GetInt32(3);
	}
}
