using Mono.Data.Sqlite;

public class UnitBaseItemData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public UnitId UnitId
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public ItemId ItemIdMutation
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        UnitId = (UnitId)reader.GetInt32(1);
        UnitSlotId = (UnitSlotId)reader.GetInt32(2);
        ItemId = (ItemId)reader.GetInt32(3);
        ItemIdMutation = (ItemId)reader.GetInt32(4);
    }
}
