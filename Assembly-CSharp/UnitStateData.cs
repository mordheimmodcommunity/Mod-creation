using Mono.Data.Sqlite;

public class UnitStateData : DataCore
{
    public UnitStateId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public string Fx
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitStateId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Fx = reader.GetString(2);
    }
}
