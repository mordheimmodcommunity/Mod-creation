using Mono.Data.Sqlite;

public class WarbandSkillFreeOutsiderItemData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandSkillFreeOutsiderId WarbandSkillFreeOutsiderId
	{
		get;
		private set;
	}

	public UnitSlotId UnitSlotId
	{
		get;
		private set;
	}

	public ItemId ItemId
	{
		get;
		private set;
	}

	public ItemQualityId ItemQualityId
	{
		get;
		private set;
	}

	public RuneMarkId RuneMarkId
	{
		get;
		private set;
	}

	public RuneMarkQualityId RuneMarkQualityId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandSkillFreeOutsiderId = (WarbandSkillFreeOutsiderId)reader.GetInt32(1);
		UnitSlotId = (UnitSlotId)reader.GetInt32(2);
		ItemId = (ItemId)reader.GetInt32(3);
		ItemQualityId = (ItemQualityId)reader.GetInt32(4);
		RuneMarkId = (RuneMarkId)reader.GetInt32(5);
		RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(6);
	}
}
