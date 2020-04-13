using Mono.Data.Sqlite;

public class AchievementTargetData : DataCore
{
    public AchievementTargetId Id
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
        Id = (AchievementTargetId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
