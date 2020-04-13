using Mono.Data.Sqlite;

public class ProcWarbandRankJoinItemQualityData : DataCore
{
    public ProcWarbandRankId ProcWarbandRankId
    {
        get;
        private set;
    }

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public int MinCount
    {
        get;
        private set;
    }

    public int MaxCount
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        ProcWarbandRankId = (ProcWarbandRankId)reader.GetInt32(0);
        ItemQualityId = (ItemQualityId)reader.GetInt32(1);
        MinCount = reader.GetInt32(2);
        MaxCount = reader.GetInt32(3);
    }
}
