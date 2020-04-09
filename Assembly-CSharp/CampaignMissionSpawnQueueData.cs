using Mono.Data.Sqlite;

public class CampaignMissionSpawnQueueData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public CampaignMissionSpawnId CampaignMissionSpawnId
	{
		get;
		private set;
	}

	public int Order
	{
		get;
		private set;
	}

	public UnitTypeId UnitTypeId
	{
		get;
		private set;
	}

	public int Amount
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		CampaignMissionSpawnId = (CampaignMissionSpawnId)reader.GetInt32(1);
		Order = reader.GetInt32(2);
		UnitTypeId = (UnitTypeId)reader.GetInt32(3);
		Amount = reader.GetInt32(4);
		Rank = reader.GetInt32(5);
		Rating = reader.GetInt32(6);
	}
}
