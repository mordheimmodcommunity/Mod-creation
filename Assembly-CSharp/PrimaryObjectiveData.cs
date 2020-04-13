using Mono.Data.Sqlite;

public class PrimaryObjectiveData : DataCore
{
    public PrimaryObjectiveId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public PrimaryObjectiveTypeId PrimaryObjectiveTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (PrimaryObjectiveId)reader.GetInt32(0);
        Name = reader.GetString(1);
        PrimaryObjectiveTypeId = (PrimaryObjectiveTypeId)reader.GetInt32(2);
    }
}
