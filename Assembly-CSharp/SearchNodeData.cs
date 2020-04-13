using Mono.Data.Sqlite;

public class SearchNodeData : DataCore
{
    public SearchNodeId Id
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
        Id = (SearchNodeId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
