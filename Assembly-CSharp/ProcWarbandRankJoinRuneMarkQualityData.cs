using Mono.Data.Sqlite;

public class ProcWarbandRankJoinRuneMarkQualityData : DataCore
{
    public ProcWarbandRankId ProcWarbandRankId
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityId
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

    public override void Populate(SqliteDataReader reader)
    {
        ProcWarbandRankId = (ProcWarbandRankId)reader.GetInt32(0);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(1);
        MinCount = reader.GetInt32(2);
        MaxCount = reader.GetInt32(3);
    }
}
