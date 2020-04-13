using Mono.Data.Sqlite;

public class WarbandChestData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public WarbandId WarbandId
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public int Count
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        WarbandId = (WarbandId)reader.GetInt32(1);
        ItemId = (ItemId)reader.GetInt32(2);
        Count = reader.GetInt32(3);
    }
}
