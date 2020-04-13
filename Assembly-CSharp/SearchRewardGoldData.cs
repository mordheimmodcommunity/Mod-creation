using Mono.Data.Sqlite;

public class SearchRewardGoldData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public SearchRewardId SearchRewardId
    {
        get;
        private set;
    }

    public int WarbandRank
    {
        get;
        private set;
    }

    public int Min
    {
        get;
        private set;
    }

    public int Max
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        SearchRewardId = (SearchRewardId)reader.GetInt32(1);
        WarbandRank = reader.GetInt32(2);
        Min = reader.GetInt32(3);
        Max = reader.GetInt32(4);
    }
}
