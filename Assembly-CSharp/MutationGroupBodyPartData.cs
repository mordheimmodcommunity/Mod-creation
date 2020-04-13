using Mono.Data.Sqlite;

public class MutationGroupBodyPartData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public MutationGroupId MutationGroupId
    {
        get;
        private set;
    }

    public BodyPartId BodyPartId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        MutationGroupId = (MutationGroupId)reader.GetInt32(1);
        BodyPartId = (BodyPartId)reader.GetInt32(2);
    }
}
