using Mono.Data.Sqlite;

public class UnitJoinPerkData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public PerkId PerkId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        PerkId = (PerkId)reader.GetInt32(1);
    }
}
