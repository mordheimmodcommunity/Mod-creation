using Mono.Data.Sqlite;

public class RaceData : DataCore
{
    public RaceId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public ItemId ItemIdTrophy
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (RaceId)reader.GetInt32(0);
        Name = reader.GetString(1);
        ItemIdTrophy = (ItemId)reader.GetInt32(2);
    }
}
