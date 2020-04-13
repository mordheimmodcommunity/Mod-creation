using Mono.Data.Sqlite;

public class TrapEffectData : DataCore
{
    public TrapEffectId Id
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

    public ZoneAoeId ZoneAoeId
    {
        get;
        private set;
    }

    public double Radius
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (TrapEffectId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Fx = reader.GetString(2);
        ZoneAoeId = (ZoneAoeId)reader.GetInt32(3);
        Radius = reader.GetDouble(4);
    }
}
