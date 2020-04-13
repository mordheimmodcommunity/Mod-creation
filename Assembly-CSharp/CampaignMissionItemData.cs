using Mono.Data.Sqlite;

public class CampaignMissionItemData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

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

    public AllegianceId AllegianceId
    {
        get;
        private set;
    }

    public int Amount
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        CampaignMissionId = (CampaignMissionId)reader.GetInt32(1);
        CampaignWarbandId = (CampaignWarbandId)reader.GetInt32(2);
        ItemId = (ItemId)reader.GetInt32(3);
        ItemQualityId = (ItemQualityId)reader.GetInt32(4);
        RuneMarkId = (RuneMarkId)reader.GetInt32(5);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(6);
        AllegianceId = (AllegianceId)reader.GetInt32(7);
        Amount = reader.GetInt32(8);
    }
}
