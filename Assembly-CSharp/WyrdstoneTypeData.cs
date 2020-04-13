using Mono.Data.Sqlite;

public class WyrdstoneTypeData : DataCore
{
    public WyrdstoneTypeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (WyrdstoneTypeId)reader.GetInt32(0);
        Name = reader.GetString(1);
        ItemId = (ItemId)reader.GetInt32(2);
    }
}
