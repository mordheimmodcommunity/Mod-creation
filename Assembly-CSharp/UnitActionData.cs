using Mono.Data.Sqlite;

public class UnitActionData : DataCore
{
    public UnitActionId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public bool Confirmation
    {
        get;
        private set;
    }

    public bool Interactive
    {
        get;
        private set;
    }

    public UnitActionRefreshId UnitActionRefreshId
    {
        get;
        private set;
    }

    public int SortWeight
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitActionId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Confirmation = (reader.GetInt32(2) != 0);
        Interactive = (reader.GetInt32(3) != 0);
        UnitActionRefreshId = (UnitActionRefreshId)reader.GetInt32(4);
        SortWeight = reader.GetInt32(5);
    }
}
