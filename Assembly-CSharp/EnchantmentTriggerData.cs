using Mono.Data.Sqlite;

public class EnchantmentTriggerData : DataCore
{
    public EnchantmentTriggerId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (EnchantmentTriggerId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
