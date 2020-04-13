using Mono.Data.Sqlite;

public class AchievementCategoryData : DataCore
{
    public AchievementCategoryId Id
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
        Id = (AchievementCategoryId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
