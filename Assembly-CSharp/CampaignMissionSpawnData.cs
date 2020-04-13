using Mono.Data.Sqlite;

public class CampaignMissionSpawnData : DataCore
{
    public CampaignMissionSpawnId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public CampaignMissionId CampaignMissionId
    {
        get;
        private set;
    }

    public int Team
    {
        get;
        private set;
    }

    public int MinUnit
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (CampaignMissionSpawnId)reader.GetInt32(0);
        Name = reader.GetString(1);
        CampaignMissionId = (CampaignMissionId)reader.GetInt32(2);
        Team = reader.GetInt32(3);
        MinUnit = reader.GetInt32(4);
    }
}
