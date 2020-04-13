using Mono.Data.Sqlite;

public class WarbandContactData : DataCore
{
    public WarbandContactId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (WarbandContactId)reader.GetInt32(0);
        Name = reader.GetString(1);
        ItemTypeId = (ItemTypeId)reader.GetInt32(2);
    }
}
