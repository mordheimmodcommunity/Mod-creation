using Mono.Data.Sqlite;

public class ItemUnitData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public UnitId UnitId
    {
        get;
        private set;
    }

    public bool Mutation
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        ItemId = (ItemId)reader.GetInt32(1);
        UnitId = (UnitId)reader.GetInt32(2);
        Mutation = (reader.GetInt32(3) != 0);
    }
}
