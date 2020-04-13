using Mono.Data.Sqlite;

public class UnitRigData : DataCore
{
    public UnitRigId Id
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
        Id = (UnitRigId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
