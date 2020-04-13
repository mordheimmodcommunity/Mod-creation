using Mono.Data.Sqlite;

public class SkillTypeData : DataCore
{
    public SkillTypeId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (SkillTypeId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
