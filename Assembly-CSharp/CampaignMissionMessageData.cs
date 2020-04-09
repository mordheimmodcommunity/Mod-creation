using Mono.Data.Sqlite;

public class CampaignMissionMessageData : DataCore
{
	public CampaignMissionMessageId Id
	{
		get;
		private set;
	}

	public string Label
	{
		get;
		private set;
	}

	public CampaignMissionId CampaignMissionId
	{
		get;
		private set;
	}

	public int UnitTurn
	{
		get;
		private set;
	}

	public int Position
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (CampaignMissionMessageId)reader.GetInt32(0);
		Label = reader.GetString(1);
		CampaignMissionId = (CampaignMissionId)reader.GetInt32(2);
		UnitTurn = reader.GetInt32(3);
		Position = reader.GetInt32(4);
		Name = reader.GetString(5);
	}
}
