using Mono.Data.Sqlite;

public class WarbandEnchantmentMarketModifierData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public WarbandEnchantmentId WarbandEnchantmentId
    {
        get;
        private set;
    }

    public MarketEventId MarketEventId
    {
        get;
        private set;
    }

    public int Modifier
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        WarbandEnchantmentId = (WarbandEnchantmentId)reader.GetInt32(1);
        MarketEventId = (MarketEventId)reader.GetInt32(2);
        Modifier = reader.GetInt32(3);
    }
}
