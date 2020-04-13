using Mono.Data.Sqlite;

public class MonthData : DataCore
{
    public MonthId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int NumDays
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (MonthId)reader.GetInt32(0);
        Name = reader.GetString(1);
        NumDays = reader.GetInt32(2);
    }
}
