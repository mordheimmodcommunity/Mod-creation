using Mono.Data.Sqlite;

public class RuneMarkJoinItemTypeData : DataCore
{
    public RuneMarkId RuneMarkId
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        RuneMarkId = (RuneMarkId)reader.GetInt32(0);
        ItemTypeId = (ItemTypeId)reader.GetInt32(1);
    }
}
