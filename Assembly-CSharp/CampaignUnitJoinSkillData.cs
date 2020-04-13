using Mono.Data.Sqlite;

public class CampaignUnitJoinSkillData : DataCore
{
    public CampaignUnitId CampaignUnitId
    {
        get;
        private set;
    }

    public SkillId SkillId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        CampaignUnitId = (CampaignUnitId)reader.GetInt32(0);
        SkillId = (SkillId)reader.GetInt32(1);
    }
}
