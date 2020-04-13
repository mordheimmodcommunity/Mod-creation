using Mono.Data.Sqlite;

public class UnitRankData : DataCore
{
    public UnitRankId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int Rank
    {
        get;
        private set;
    }

    public int Advancement
    {
        get;
        private set;
    }

    public int Rating
    {
        get;
        private set;
    }

    public int Wound
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitRankId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Rank = reader.GetInt32(2);
        Advancement = reader.GetInt32(3);
        Rating = reader.GetInt32(4);
        Wound = reader.GetInt32(5);
    }
}
