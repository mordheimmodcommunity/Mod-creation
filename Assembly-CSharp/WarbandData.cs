using Mono.Data.Sqlite;

public class WarbandData : DataCore
{
    public WarbandId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public string Asset
    {
        get;
        private set;
    }

    public bool Basic
    {
        get;
        private set;
    }

    public int MinUnitCount
    {
        get;
        private set;
    }

    public int MaxUnitCount
    {
        get;
        private set;
    }

    public UnitId RequiredUnitId
    {
        get;
        private set;
    }

    public AllegianceId AllegianceId
    {
        get;
        private set;
    }

    public string Wagon
    {
        get;
        private set;
    }

    public string Chest
    {
        get;
        private set;
    }

    public ItemId ItemIdIdol
    {
        get;
        private set;
    }

    public UnitId UnitIdDramatis
    {
        get;
        private set;
    }

    public ColorPresetId ColorPresetId
    {
        get;
        private set;
    }

    public string DefaultName
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (WarbandId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Asset = reader.GetString(2);
        Basic = (reader.GetInt32(3) != 0);
        MinUnitCount = reader.GetInt32(4);
        MaxUnitCount = reader.GetInt32(5);
        RequiredUnitId = (UnitId)reader.GetInt32(6);
        AllegianceId = (AllegianceId)reader.GetInt32(7);
        Wagon = reader.GetString(8);
        Chest = reader.GetString(9);
        ItemIdIdol = (ItemId)reader.GetInt32(10);
        UnitIdDramatis = (UnitId)reader.GetInt32(11);
        ColorPresetId = (ColorPresetId)reader.GetInt32(12);
        DefaultName = reader.GetString(13);
    }
}
