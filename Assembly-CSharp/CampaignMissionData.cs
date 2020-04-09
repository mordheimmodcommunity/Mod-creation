using Mono.Data.Sqlite;

public class CampaignMissionData : DataCore
{
	public CampaignMissionId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public bool IsTuto
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public int Idx
	{
		get;
		private set;
	}

	public DeploymentScenarioId DeploymentScenarioId
	{
		get;
		private set;
	}

	public int MapPos
	{
		get;
		private set;
	}

	public int Days
	{
		get;
		private set;
	}

	public WarbandSkillId WarbandSkillIdReward
	{
		get;
		private set;
	}

	public SearchDensityId SearchDensityId
	{
		get;
		private set;
	}

	public WyrdstonePlacementId WyrdstonePlacementId
	{
		get;
		private set;
	}

	public WyrdstoneDensityId WyrdstoneDensityId
	{
		get;
		private set;
	}

	public int LoadingImageCount
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (CampaignMissionId)reader.GetInt32(0);
		Name = reader.GetString(1);
		IsTuto = (reader.GetInt32(2) != 0);
		WarbandId = (WarbandId)reader.GetInt32(3);
		Rating = reader.GetInt32(4);
		Idx = reader.GetInt32(5);
		DeploymentScenarioId = (DeploymentScenarioId)reader.GetInt32(6);
		MapPos = reader.GetInt32(7);
		Days = reader.GetInt32(8);
		WarbandSkillIdReward = (WarbandSkillId)reader.GetInt32(9);
		SearchDensityId = (SearchDensityId)reader.GetInt32(10);
		WyrdstonePlacementId = (WyrdstonePlacementId)reader.GetInt32(11);
		WyrdstoneDensityId = (WyrdstoneDensityId)reader.GetInt32(12);
		LoadingImageCount = reader.GetInt32(13);
	}
}
