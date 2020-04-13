using Mono.Data.Sqlite;

public class AchievementData : DataCore
{
    public AchievementId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public AchievementCategoryId AchievementCategoryId
    {
        get;
        private set;
    }

    public AchievementTargetId AchievementTargetId
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public WarbandId WarbandId
    {
        get;
        private set;
    }

    public AchievementTypeId AchievementTypeId
    {
        get;
        private set;
    }

    public AchievementId AchievementIdRequire
    {
        get;
        private set;
    }

    public WarbandAttributeId WarbandAttributeId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public CampaignMissionId CampaignMissionId
    {
        get;
        private set;
    }

    public int TargetStatValue
    {
        get;
        private set;
    }

    public bool PlayerProgression
    {
        get;
        private set;
    }

    public bool EndReportVisible
    {
        get;
        private set;
    }

    public int Xp
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (AchievementId)reader.GetInt32(0);
        Name = reader.GetString(1);
        AchievementCategoryId = (AchievementCategoryId)reader.GetInt32(2);
        AchievementTargetId = (AchievementTargetId)reader.GetInt32(3);
        UnitTypeId = (UnitTypeId)reader.GetInt32(4);
        WarbandId = (WarbandId)reader.GetInt32(5);
        AchievementTypeId = (AchievementTypeId)reader.GetInt32(6);
        AchievementIdRequire = (AchievementId)reader.GetInt32(7);
        WarbandAttributeId = (WarbandAttributeId)reader.GetInt32(8);
        AttributeId = (AttributeId)reader.GetInt32(9);
        CampaignMissionId = (CampaignMissionId)reader.GetInt32(10);
        TargetStatValue = reader.GetInt32(11);
        PlayerProgression = (reader.GetInt32(12) != 0);
        EndReportVisible = (reader.GetInt32(13) != 0);
        Xp = reader.GetInt32(14);
    }
}
