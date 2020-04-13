using Mono.Data.Sqlite;

public class WarbandSkillWarbandEnchantmentData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public WarbandSkillId WarbandSkillId
    {
        get;
        private set;
    }

    public WarbandEnchantmentId WarbandEnchantmentId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        WarbandSkillId = (WarbandSkillId)reader.GetInt32(1);
        WarbandEnchantmentId = (WarbandEnchantmentId)reader.GetInt32(2);
    }
}
