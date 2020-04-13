using Mono.Data.Sqlite;

public class PrimaryObjectiveWantedData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public PrimaryObjectiveId PrimaryObjectiveId
    {
        get;
        private set;
    }

    public CampaignUnitId CampaignUnitId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(1);
        CampaignUnitId = (CampaignUnitId)reader.GetInt32(2);
    }
}
