using Mono.Data.Sqlite;

public class WarbandSkillMarketItemData : DataCore
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

	public ItemId ItemId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandSkillId = (WarbandSkillId)reader.GetInt32(1);
		ItemId = (ItemId)reader.GetInt32(2);
	}
}
