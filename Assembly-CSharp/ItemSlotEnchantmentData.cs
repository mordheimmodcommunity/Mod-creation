using Mono.Data.Sqlite;

public class ItemSlotEnchantmentData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        ItemId = (ItemId)reader.GetInt32(1);
        UnitSlotId = (UnitSlotId)reader.GetInt32(2);
        EnchantmentId = (EnchantmentId)reader.GetInt32(3);
    }
}
