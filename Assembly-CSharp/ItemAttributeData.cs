using Mono.Data.Sqlite;

public class ItemAttributeData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public ItemId ItemId
    {
        get;
        private set;
    }

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public MutationId MutationId
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
        ItemId = (ItemId)reader.GetInt32(1);
        ItemQualityId = (ItemQualityId)reader.GetInt32(2);
        MutationId = (MutationId)reader.GetInt32(3);
        AttributeId = (AttributeId)reader.GetInt32(4);
        UnitActionIdTrigger = (UnitActionId)reader.GetInt32(5);
        SkillIdTrigger = (SkillId)reader.GetInt32(6);
        Modifier = reader.GetInt32(7);
    }
}
