using Mono.Data.Sqlite;

public class ConstantData : DataCore
{
    public ConstantId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public string Value
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (ConstantId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Value = reader.GetString(2);
    }
}
