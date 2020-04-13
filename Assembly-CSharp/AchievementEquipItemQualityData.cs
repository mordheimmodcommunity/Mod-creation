using Mono.Data.Sqlite;

public class AchievementEquipItemQualityData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public AchievementId AchievementId
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        AchievementId = (AchievementId)reader.GetInt32(1);
        UnitTypeId = (UnitTypeId)reader.GetInt32(2);
        ItemQualityId = (ItemQualityId)reader.GetInt32(3);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(4);
    }
}
