using Mono.Data.Sqlite;

public class UnitBaseData : DataCore
{
    public UnitBaseId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public UnitRigId UnitRigId
    {
        get;
        private set;
    }

    public string CamBase
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitBaseId)reader.GetInt32(0);
        Name = reader.GetString(1);
        UnitRigId = (UnitRigId)reader.GetInt32(2);
        CamBase = reader.GetString(3);
    }
}
