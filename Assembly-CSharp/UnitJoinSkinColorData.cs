using Mono.Data.Sqlite;

public class UnitJoinSkinColorData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public SkinColorId SkinColorId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        SkinColorId = (SkinColorId)reader.GetInt32(1);
    }
}
