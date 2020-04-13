using Mono.Data.Sqlite;

public class ItemEnchantmentData : DataCore
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

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public MutationId MutationId
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
        ItemQualityId = (ItemQualityId)reader.GetInt32(2);
        MutationId = (MutationId)reader.GetInt32(3);
        EnchantmentId = (EnchantmentId)reader.GetInt32(4);
    }
}
