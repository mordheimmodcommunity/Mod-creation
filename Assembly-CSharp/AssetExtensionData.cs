using Mono.Data.Sqlite;

public class AssetExtensionData : DataCore
{
    public AssetExtensionId Id
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
        Id = (AssetExtensionId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
