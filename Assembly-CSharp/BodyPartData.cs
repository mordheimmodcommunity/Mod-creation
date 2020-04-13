using Mono.Data.Sqlite;

public class BodyPartData : DataCore
{
    public BodyPartId Id
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

    public bool Customizable
    {
        get;
        private set;
    }

    public bool Empty
    {
        get;
        private set;
    }

    public bool Skinnable
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (BodyPartId)reader.GetInt32(0);
        Name = reader.GetString(1);
        UnitSlotId = (UnitSlotId)reader.GetInt32(2);
        Customizable = (reader.GetInt32(3) != 0);
        Empty = (reader.GetInt32(4) != 0);
        Skinnable = (reader.GetInt32(5) != 0);
    }
}
