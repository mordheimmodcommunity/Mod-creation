using Mono.Data.Sqlite;

public class TrapTypeJoinTrapData : DataCore
{
    public TrapTypeId TrapTypeId
    {
        get;
        private set;
    }

    public TrapId TrapId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        TrapTypeId = (TrapTypeId)reader.GetInt32(0);
        TrapId = (TrapId)reader.GetInt32(1);
    }
}
