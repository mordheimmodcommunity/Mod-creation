using Mono.Data.Sqlite;

public class CampaignUnitData : DataCore
{
	public CampaignUnitId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public AiProfileId AiProfileId
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public string FirstName
	{
		get;
		private set;
	}

	public bool StartHidden
	{
		get;
		private set;
	}

	public bool StartInactive
	{
		get;
		private set;
	}

	public CampaignUnitId CampaignUnitIdSpawnOnDeath
	{
		get;
		private set;
	}

	public bool NoLootBag
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (CampaignUnitId)reader.GetInt32(0);
		Name = reader.GetString(1);
		UnitId = (UnitId)reader.GetInt32(2);
		AiProfileId = (AiProfileId)reader.GetInt32(3);
		Rank = reader.GetInt32(4);
		FirstName = reader.GetString(5);
		StartHidden = (reader.GetInt32(6) != 0);
		StartInactive = (reader.GetInt32(7) != 0);
		CampaignUnitIdSpawnOnDeath = (CampaignUnitId)reader.GetInt32(8);
		NoLootBag = (reader.GetInt32(9) != 0);
	}
}
