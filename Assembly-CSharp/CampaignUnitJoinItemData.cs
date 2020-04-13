using Mono.Data.Sqlite;

public class CampaignUnitJoinItemData : DataCore
{
    public CampaignUnitId CampaignUnitId
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public RuneMarkId RuneMarkId
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        CampaignUnitId = (CampaignUnitId)reader.GetInt32(0);
        UnitSlotId = (UnitSlotId)reader.GetInt32(1);
        ItemId = (ItemId)reader.GetInt32(2);
        ItemQualityId = (ItemQualityId)reader.GetInt32(3);
        RuneMarkId = (RuneMarkId)reader.GetInt32(4);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(5);
    }
}
