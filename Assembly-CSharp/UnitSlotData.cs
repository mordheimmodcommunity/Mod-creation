using Mono.Data.Sqlite;

public class UnitSlotData : DataCore
{
    public UnitSlotId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public UnitSlotTypeId UnitSlotTypeId
    {
        get;
        private set;
    }

    public BoneId BoneId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitSlotId)reader.GetInt32(0);
        Name = reader.GetString(1);
        UnitSlotTypeId = (UnitSlotTypeId)reader.GetInt32(2);
        BoneId = (BoneId)reader.GetInt32(3);
    }
}
