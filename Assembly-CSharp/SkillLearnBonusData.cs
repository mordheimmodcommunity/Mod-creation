using Mono.Data.Sqlite;

public class SkillLearnBonusData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public SkillId SkillId
    {
        get;
        private set;
    }

    public int Physical
    {
        get;
        private set;
    }

    public int Mental
    {
        get;
        private set;
    }

    public int Martial
    {
        get;
        private set;
    }

    public int Skill
    {
        get;
        private set;
    }

    public int Spell
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        SkillId = (SkillId)reader.GetInt32(1);
        Physical = reader.GetInt32(2);
        Mental = reader.GetInt32(3);
        Martial = reader.GetInt32(4);
        Skill = reader.GetInt32(5);
        Spell = reader.GetInt32(6);
        UnitTypeId = (UnitTypeId)reader.GetInt32(7);
    }
}
