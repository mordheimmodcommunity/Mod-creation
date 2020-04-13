using Mono.Data.Sqlite;

public class UnitTypeAttributeData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public int Max
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        UnitTypeId = (UnitTypeId)reader.GetInt32(1);
        AttributeId = (AttributeId)reader.GetInt32(2);
        Max = reader.GetInt32(3);
    }
}
