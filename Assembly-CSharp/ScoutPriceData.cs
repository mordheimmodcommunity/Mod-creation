using Mono.Data.Sqlite;

public class ScoutPriceData : DataCore
{
    public int WarbandRank
    {
        get;
        private set;
    }

    public int Idx
    {
        get;
        private set;
    }

    public int Price
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        WarbandRank = reader.GetInt32(0);
        Idx = reader.GetInt32(1);
        Price = reader.GetInt32(2);
    }
}
