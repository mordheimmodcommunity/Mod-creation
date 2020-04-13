using Mono.Data.Sqlite;

public class PrimaryObjectiveResultData : DataCore
{
    public PrimaryObjectiveResultId Id
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
        Id = (PrimaryObjectiveResultId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
