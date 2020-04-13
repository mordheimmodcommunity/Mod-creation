using Mono.Data.Sqlite;

public class UnitSizeData : DataCore
{
    public UnitSizeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public string Size
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitSizeId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Size = reader.GetString(2);
    }
}
