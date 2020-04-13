using Mono.Data.Sqlite;

public class UnitJoinMutationData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public MutationId MutationId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        MutationId = (MutationId)reader.GetInt32(1);
    }
}
