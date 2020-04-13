using Mono.Data.Sqlite;

public class PerkData : DataCore
{
    public PerkId Id
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
        Id = (PerkId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
