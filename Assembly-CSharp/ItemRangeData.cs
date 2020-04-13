using Mono.Data.Sqlite;

public class ItemRangeData : DataCore
{
    public ItemRangeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int MinRange
    {
        get;
        private set;
    }

    public int Range
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (ItemRangeId)reader.GetInt32(0);
        Name = reader.GetString(1);
        MinRange = reader.GetInt32(2);
        Range = reader.GetInt32(3);
    }
}
