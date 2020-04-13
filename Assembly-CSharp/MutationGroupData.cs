using Mono.Data.Sqlite;

public class MutationGroupData : DataCore
{
    public MutationGroupId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public bool EmptyLinkedBodyPart
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (MutationGroupId)reader.GetInt32(0);
        Name = reader.GetString(1);
        UnitSlotId = (UnitSlotId)reader.GetInt32(2);
        EmptyLinkedBodyPart = (reader.GetInt32(3) != 0);
    }
}
