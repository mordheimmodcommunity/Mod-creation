using Mono.Data.Sqlite;

public class TrapTypeJoinTrapEffectData : DataCore
{
    public TrapTypeId TrapTypeId
    {
        get;
        private set;
    }

    public TrapEffectId TrapEffectId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        TrapTypeId = (TrapTypeId)reader.GetInt32(0);
        TrapEffectId = (TrapEffectId)reader.GetInt32(1);
    }
}
