using Mono.Data.Sqlite;

public class CampaignMissionObjectiveData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public CampaignMissionId CampaignMissionId
	{
		get;
		private set;
	}

	public PrimaryObjectiveId PrimaryObjectiveId
	{
		get;
		private set;
	}

	public int SortWeight
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		CampaignMissionId = (CampaignMissionId)reader.GetInt32(1);
		PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(2);
		SortWeight = reader.GetInt32(3);
	}
}
