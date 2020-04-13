using Mono.Data.Sqlite;

public class MoonData : DataCore
{
    public MoonId Id
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
        Id = (MoonId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
