using Mono.Data.Sqlite;

public class CampaignMissionJoinCampaignWarbandData : DataCore
{
    public CampaignMissionId CampaignMissionId
    {
        get;
        private set;
    }

    public CampaignWarbandId CampaignWarbandId
    {
        get;
        private set;
    }

    public string DeployZone
    {
        get;
        private set;
    }

    public int Team
    {
        get;
        private set;
    }

    public PlayerTypeId PlayerTypeId
    {
        get;
        private set;
    }

    public bool CanRout
    {
        get;
        private set;
    }

    public bool Objective
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        CampaignMissionId = (CampaignMissionId)reader.GetInt32(0);
        CampaignWarbandId = (CampaignWarbandId)reader.GetInt32(1);
        DeployZone = reader.GetString(2);
        Team = reader.GetInt32(3);
        PlayerTypeId = (PlayerTypeId)reader.GetInt32(4);
        CanRout = (reader.GetInt32(5) != 0);
        Objective = (reader.GetInt32(6) != 0);
    }
}
