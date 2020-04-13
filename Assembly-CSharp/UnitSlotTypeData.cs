using Mono.Data.Sqlite;

public class UnitSlotTypeData : DataCore
{
    public UnitSlotTypeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitSlotTypeId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
