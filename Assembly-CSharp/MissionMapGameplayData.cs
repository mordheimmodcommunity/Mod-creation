using Mono.Data.Sqlite;

public class MissionMapGameplayData : DataCore
{
    public MissionMapGameplayId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public MissionMapId MissionMapId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (MissionMapGameplayId)reader.GetInt32(0);
        Name = reader.GetString(1);
        MissionMapId = (MissionMapId)reader.GetInt32(2);
    }
}
