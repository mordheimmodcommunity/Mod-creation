using Mono.Data.Sqlite;

public class WarbandSkillItemData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public WarbandSkillId WarbandSkillId
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

    public int Quantity
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        WarbandSkillId = (WarbandSkillId)reader.GetInt32(1);
        ItemId = (ItemId)reader.GetInt32(2);
        ItemQualityId = (ItemQualityId)reader.GetInt32(3);
        RuneMarkId = (RuneMarkId)reader.GetInt32(4);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(5);
        Quantity = reader.GetInt32(6);
    }
}
