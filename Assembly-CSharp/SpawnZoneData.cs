using Mono.Data.Sqlite;

public class SpawnZoneData : DataCore
{
    public SpawnZoneId Id
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
        Id = (SpawnZoneId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
