using Mono.Data.Sqlite;

public class ArmorStyleData : DataCore
{
    public ArmorStyleId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeIdArmor
    {
        get;
        private set;
    }

    public ItemTypeId ItemTypeIdHelmet
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (ArmorStyleId)reader.GetInt32(0);
        Name = reader.GetString(1);
        ItemTypeIdArmor = (ItemTypeId)reader.GetInt32(2);
        ItemTypeIdHelmet = (ItemTypeId)reader.GetInt32(3);
    }
}
