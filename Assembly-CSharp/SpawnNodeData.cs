using Mono.Data.Sqlite;

public class SpawnNodeData : DataCore
{
    public SpawnNodeId Id
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
        Id = (SpawnNodeId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
