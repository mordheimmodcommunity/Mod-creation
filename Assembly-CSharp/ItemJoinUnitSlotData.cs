using Mono.Data.Sqlite;

public class ItemJoinUnitSlotData : DataCore
{
    public ItemId ItemId
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        ItemId = (ItemId)reader.GetInt32(0);
        UnitSlotId = (UnitSlotId)reader.GetInt32(1);
    }
}
