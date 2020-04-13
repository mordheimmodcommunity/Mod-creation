using Mono.Data.Sqlite;

public class WarbandEnchantmentData : DataCore
{
    public WarbandEnchantmentId Id
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
        Id = (WarbandEnchantmentId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
