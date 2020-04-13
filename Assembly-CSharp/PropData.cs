using Mono.Data.Sqlite;

public class PropData : DataCore
{
    public PropId Id
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
        Id = (PropId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
