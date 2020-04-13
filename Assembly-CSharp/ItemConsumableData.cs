using Mono.Data.Sqlite;

public class ItemConsumableData : DataCore
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

    public SkillId SkillId
    {
        get;
        private set;
    }

    public bool ConsumeItem
    {
        get;
        private set;
    }

    public bool OutOfCombat
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        ItemId = (ItemId)reader.GetInt32(1);
        ItemQualityId = (ItemQualityId)reader.GetInt32(2);
        SkillId = (SkillId)reader.GetInt32(3);
        ConsumeItem = (reader.GetInt32(4) != 0);
        OutOfCombat = (reader.GetInt32(5) != 0);
    }
}
