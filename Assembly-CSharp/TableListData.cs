using Mono.Data.Sqlite;

public class TableListData : DataCore
{
    public TableListId Id
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
        Id = (TableListId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
