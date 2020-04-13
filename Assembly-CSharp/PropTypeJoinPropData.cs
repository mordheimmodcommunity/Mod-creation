using Mono.Data.Sqlite;

public class PropTypeJoinPropData : DataCore
{
    public PropTypeId PropTypeId
    {
        get;
        private set;
    }

    public PropId PropId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        PropTypeId = (PropTypeId)reader.GetInt32(0);
        PropId = (PropId)reader.GetInt32(1);
    }
}
