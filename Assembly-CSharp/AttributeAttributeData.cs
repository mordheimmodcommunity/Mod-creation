using Mono.Data.Sqlite;

public class AttributeAttributeData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public AttributeId AttributeIdBase
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
        AttributeId = (AttributeId)reader.GetInt32(1);
        AttributeIdBase = (AttributeId)reader.GetInt32(2);
        Modifier = reader.GetInt32(3);
    }
}
