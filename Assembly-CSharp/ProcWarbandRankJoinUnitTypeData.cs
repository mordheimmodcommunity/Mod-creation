using Mono.Data.Sqlite;

public class ProcWarbandRankJoinUnitTypeData : DataCore
{
    public ProcWarbandRankId ProcWarbandRankId
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public int MinCount
    {
        get;
        private set;
    }

    public int MaxCount
    {
        get;
        private set;
    }

    public int MinRank
    {
        get;
        private set;
    }

    public int MaxRank
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        ProcWarbandRankId = (ProcWarbandRankId)reader.GetInt32(0);
        UnitTypeId = (UnitTypeId)reader.GetInt32(1);
        MinCount = reader.GetInt32(2);
        MaxCount = reader.GetInt32(3);
        MinRank = reader.GetInt32(4);
        MaxRank = reader.GetInt32(5);
    }
}
