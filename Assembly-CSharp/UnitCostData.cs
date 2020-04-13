using Mono.Data.Sqlite;

public class UnitCostData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public int Rank
    {
        get;
        private set;
    }

    public int DecisiveVictory
    {
        get;
        private set;
    }

    public int PartialVictory
    {
        get;
        private set;
    }

    public int Defeat
    {
        get;
        private set;
    }

    public int Treatment
    {
        get;
        private set;
    }

    public int Hiring
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        UnitTypeId = (UnitTypeId)reader.GetInt32(1);
        Rank = reader.GetInt32(2);
        DecisiveVictory = reader.GetInt32(3);
        PartialVictory = reader.GetInt32(4);
        Defeat = reader.GetInt32(5);
        Treatment = reader.GetInt32(6);
        Hiring = reader.GetInt32(7);
    }
}
