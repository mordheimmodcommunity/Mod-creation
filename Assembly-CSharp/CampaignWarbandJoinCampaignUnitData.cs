using Mono.Data.Sqlite;

public class CampaignWarbandJoinCampaignUnitData : DataCore
{
	public CampaignWarbandId CampaignWarbandId
	{
		get;
		private set;
	}

	public CampaignUnitId CampaignUnitId
	{
		get;
		private set;
	}

	public string DeployNode
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		CampaignWarbandId = (CampaignWarbandId)reader.GetInt32(0);
		CampaignUnitId = (CampaignUnitId)reader.GetInt32(1);
		DeployNode = reader.GetString(2);
	}
}
