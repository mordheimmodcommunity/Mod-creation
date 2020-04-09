using Mono.Data.Sqlite;

public class CampaignWarbandWhitelistData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public CampaignWarbandId CampaignWarbandId
	{
		get;
		private set;
	}

	public CampaignWarbandId CampaignWarbandIdWhitelisted
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		CampaignWarbandId = (CampaignWarbandId)reader.GetInt32(1);
		CampaignWarbandIdWhitelisted = (CampaignWarbandId)reader.GetInt32(2);
	}
}
