using Mono.Data.Sqlite;

public class UnitActiveStatusData : DataCore
{
    public UnitActiveStatusId Id
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
        Id = (UnitActiveStatusId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
