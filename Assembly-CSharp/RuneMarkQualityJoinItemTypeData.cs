using Mono.Data.Sqlite;

public class RuneMarkQualityJoinItemTypeData : DataCore
{
    public RuneMarkQualityId RuneMarkQualityId
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public int Rating
    {
        get;
        private set;
    }

    public int CostModifier
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(0);
        ItemTypeId = (ItemTypeId)reader.GetInt32(1);
        Rating = reader.GetInt32(2);
        CostModifier = reader.GetInt32(3);
    }
}
