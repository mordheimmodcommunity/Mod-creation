using Mono.Data.Sqlite;

public class UnitJoinAttributeData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public int BaseValue
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        AttributeId = (AttributeId)reader.GetInt32(1);
        BaseValue = reader.GetInt32(2);
    }
}
