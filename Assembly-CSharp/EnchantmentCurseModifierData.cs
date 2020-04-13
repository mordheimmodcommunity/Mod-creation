using Mono.Data.Sqlite;

public class EnchantmentCurseModifierData : DataCore
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

    public SpellCurseId SpellCurseId
    {
        get;
        private set;
    }

    public int RatioModifier
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        EnchantmentId = (EnchantmentId)reader.GetInt32(1);
        SpellCurseId = (SpellCurseId)reader.GetInt32(2);
        RatioModifier = reader.GetInt32(3);
    }
}
