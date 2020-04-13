using Mono.Data.Sqlite;

public class VictoryTypeData : DataCore
{
    public VictoryTypeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int WarbandExperience
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (VictoryTypeId)reader.GetInt32(0);
        Name = reader.GetString(1);
        WarbandExperience = reader.GetInt32(2);
    }
}
