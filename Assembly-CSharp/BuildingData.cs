using Mono.Data.Sqlite;

public class BuildingData : DataCore
{
    public BuildingId Id
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
        Id = (BuildingId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
