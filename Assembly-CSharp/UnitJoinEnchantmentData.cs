using Mono.Data.Sqlite;

public class UnitJoinEnchantmentData : DataCore
{
    public UnitId UnitId
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
        UnitId = (UnitId)reader.GetInt32(0);
        EnchantmentId = (EnchantmentId)reader.GetInt32(1);
    }
}
