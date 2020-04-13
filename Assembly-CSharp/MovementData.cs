using Mono.Data.Sqlite;

public class MovementData : DataCore
{
    public MovementId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int Distance
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (MovementId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Distance = reader.GetInt32(2);
    }
}
