using Mono.Data.Sqlite;

public class CampaignUnitJoinAttributeData : DataCore
{
    public CampaignUnitId CampaignUnitId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public int Value
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        CampaignUnitId = (CampaignUnitId)reader.GetInt32(0);
        AttributeId = (AttributeId)reader.GetInt32(1);
        Value = reader.GetInt32(2);
    }
}
