using Mono.Data.Sqlite;

public class InjuryJoinEnchantmentData : DataCore
{
    public InjuryId InjuryId
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
        InjuryId = (InjuryId)reader.GetInt32(0);
        EnchantmentId = (EnchantmentId)reader.GetInt32(1);
    }
}
