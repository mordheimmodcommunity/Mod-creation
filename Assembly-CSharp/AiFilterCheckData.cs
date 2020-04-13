using Mono.Data.Sqlite;

public class AiFilterCheckData : DataCore
{
    public AiFilterCheckId Id
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
        Id = (AiFilterCheckId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
