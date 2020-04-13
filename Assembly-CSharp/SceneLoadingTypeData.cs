using Mono.Data.Sqlite;

public class SceneLoadingTypeData : DataCore
{
    public SceneLoadingTypeId Id
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
        Id = (SceneLoadingTypeId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
