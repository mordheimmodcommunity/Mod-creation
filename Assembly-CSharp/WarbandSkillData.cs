using Mono.Data.Sqlite;

public class WarbandSkillData : DataCore
{
    public WarbandSkillId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public WarbandSkillTypeId WarbandSkillTypeId
    {
        get;
        private set;
    }

    public SkillQualityId SkillQualityId
    {
        get;
        private set;
    }

    public WarbandSkillId WarbandSkillIdPrerequisite
    {
        get;
        private set;
    }

    public int Points
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (WarbandSkillId)reader.GetInt32(0);
        Name = reader.GetString(1);
        WarbandSkillTypeId = (WarbandSkillTypeId)reader.GetInt32(2);
        SkillQualityId = (SkillQualityId)reader.GetInt32(3);
        WarbandSkillIdPrerequisite = (WarbandSkillId)reader.GetInt32(4);
        Points = reader.GetInt32(5);
    }
}
