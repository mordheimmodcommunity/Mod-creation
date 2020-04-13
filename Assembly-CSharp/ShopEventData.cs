using Mono.Data.Sqlite;

public class ShopEventData : DataCore
{
    public ShopEventId Id
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
        Id = (ShopEventId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
