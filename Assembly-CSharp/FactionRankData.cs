using Mono.Data.Sqlite;

public class FactionRankData : DataCore
{
    public FactionRankId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int Reputation
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (FactionRankId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Reputation = reader.GetInt32(2);
    }
}
