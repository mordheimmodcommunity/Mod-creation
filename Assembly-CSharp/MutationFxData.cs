using Mono.Data.Sqlite;

public class MutationFxData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public MutationId MutationId
    {
        get;
        private set;
    }

    public UnitId UnitId
    {
        get;
        private set;
    }

    public string Asset
    {
        get;
        private set;
    }

    public string Trail
    {
        get;
        private set;
    }

    public BoneId BoneIdTrail
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        MutationId = (MutationId)reader.GetInt32(1);
        UnitId = (UnitId)reader.GetInt32(2);
        Asset = reader.GetString(3);
        Trail = reader.GetString(4);
        BoneIdTrail = (BoneId)reader.GetInt32(5);
    }
}
