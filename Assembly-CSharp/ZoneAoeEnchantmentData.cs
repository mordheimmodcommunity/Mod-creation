using Mono.Data.Sqlite;

public class ZoneAoeEnchantmentData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public ZoneAoeId ZoneAoeId
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentId
    {
        get;
        private set;
    }

    public ZoneTriggerId ZoneTriggerId
    {
        get;
        private set;
    }

    public bool TargetSelf
    {
        get;
        private set;
    }

    public bool TargetAlly
    {
        get;
        private set;
    }

    public bool TargetEnemy
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        ZoneAoeId = (ZoneAoeId)reader.GetInt32(1);
        EnchantmentId = (EnchantmentId)reader.GetInt32(2);
        ZoneTriggerId = (ZoneTriggerId)reader.GetInt32(3);
        TargetSelf = (reader.GetInt32(4) != 0);
        TargetAlly = (reader.GetInt32(5) != 0);
        TargetEnemy = (reader.GetInt32(6) != 0);
    }
}
