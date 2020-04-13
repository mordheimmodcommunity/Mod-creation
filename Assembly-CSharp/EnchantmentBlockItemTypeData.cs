using Mono.Data.Sqlite;

public class EnchantmentBlockItemTypeData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentId
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        EnchantmentId = (EnchantmentId)reader.GetInt32(1);
        ItemTypeId = (ItemTypeId)reader.GetInt32(2);
    }
}
