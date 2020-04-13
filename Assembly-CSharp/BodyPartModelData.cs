using Mono.Data.Sqlite;

public class BodyPartModelData : DataCore
{
    public BodyPartModelId Id
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
        Id = (BodyPartModelId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
