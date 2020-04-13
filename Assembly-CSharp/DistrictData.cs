using Mono.Data.Sqlite;

public class DistrictData : DataCore
{
    public DistrictId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int Slots
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (DistrictId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Slots = reader.GetInt32(2);
    }
}
