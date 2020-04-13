using Mono.Data.Sqlite;

public class ItemQualityData : DataCore
{
    public ItemQualityId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int EnchantSlots
    {
        get;
        private set;
    }

    public int PriceBuyModifier
    {
        get;
        private set;
    }

    public int PriceSoldModifier
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityIdMax
    {
        get;
        private set;
    }

    public string Color
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (ItemQualityId)reader.GetInt32(0);
        Name = reader.GetString(1);
        EnchantSlots = reader.GetInt32(2);
        PriceBuyModifier = reader.GetInt32(3);
        PriceSoldModifier = reader.GetInt32(4);
        RuneMarkQualityIdMax = (RuneMarkQualityId)reader.GetInt32(5);
        Color = reader.GetString(6);
    }
}
