using Mono.Data.Sqlite;

public class CampaignWarbandData : DataCore
{
    public CampaignWarbandId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public WarbandId WarbandId
    {
        get;
        private set;
    }

    public ColorPresetId ColorPresetId
    {
        get;
        private set;
    }

    public int Rank
    {
        get;
        private set;
    }

    public bool NoWagon
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (CampaignWarbandId)reader.GetInt32(0);
        Name = reader.GetString(1);
        WarbandId = (WarbandId)reader.GetInt32(2);
        ColorPresetId = (ColorPresetId)reader.GetInt32(3);
        Rank = reader.GetInt32(4);
        NoWagon = (reader.GetInt32(5) != 0);
    }
}
