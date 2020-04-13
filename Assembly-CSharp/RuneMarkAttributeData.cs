using Mono.Data.Sqlite;

public class RuneMarkAttributeData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public RuneMarkId RuneMarkId
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityId
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public UnitActionId UnitActionId
    {
        get;
        private set;
    }

    public SkillId SkillId
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
        RuneMarkId = (RuneMarkId)reader.GetInt32(1);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(2);
        ItemTypeId = (ItemTypeId)reader.GetInt32(3);
        AttributeId = (AttributeId)reader.GetInt32(4);
        UnitActionId = (UnitActionId)reader.GetInt32(5);
        SkillId = (SkillId)reader.GetInt32(6);
        Modifier = reader.GetInt32(7);
    }
}
