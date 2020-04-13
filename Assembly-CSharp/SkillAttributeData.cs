using Mono.Data.Sqlite;

public class SkillAttributeData : DataCore
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

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public UnitActionId UnitActionIdTrigger
    {
        get;
        private set;
    }

    public SkillId SkillIdTrigger
    {
        get;
        private set;
    }

    public int Modifier
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        SkillId = (SkillId)reader.GetInt32(1);
        AttributeId = (AttributeId)reader.GetInt32(2);
        UnitActionIdTrigger = (UnitActionId)reader.GetInt32(3);
        SkillIdTrigger = (SkillId)reader.GetInt32(4);
        Modifier = reader.GetInt32(5);
    }
}
