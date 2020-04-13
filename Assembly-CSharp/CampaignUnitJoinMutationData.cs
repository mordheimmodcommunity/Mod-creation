using Mono.Data.Sqlite;

public class CampaignUnitJoinMutationData : DataCore
{
    public CampaignUnitId CampaignUnitId
    {
        get;
        private set;
    }

    public MutationId MutationId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        CampaignUnitId = (CampaignUnitId)reader.GetInt32(0);
        MutationId = (MutationId)reader.GetInt32(1);
    }
}
