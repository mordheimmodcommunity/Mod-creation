using Mono.Data.Sqlite;

public class UnitJoinSkillLineData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public SkillLineId SkillLineId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        SkillLineId = (SkillLineId)reader.GetInt32(1);
    }
}
