using Mono.Data.Sqlite;

public class UnitData : DataCore
{
    public UnitId Id
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

    public string AltAsset
    {
        get;
        private set;
    }

    public bool Base
    {
        get;
        private set;
    }

    public bool Released
    {
        get;
        private set;
    }

    public RaceId RaceId
    {
        get;
        private set;
    }

    public WarbandId WarbandId
    {
        get;
        private set;
    }

    public int MaxCount
    {
        get;
        private set;
    }

    public UnitTypeId UnitTypeId
    {
        get;
        private set;
    }

    public UnitSizeId UnitSizeId
    {
        get;
        private set;
    }

    public MovementId MovementId
    {
        get;
        private set;
    }

    public UnitWoundId UnitWoundId
    {
        get;
        private set;
    }

    public UnitBaseId UnitBaseId
    {
        get;
        private set;
    }

    public ItemId ItemIdTrophy
    {
        get;
        private set;
    }

    public AllegianceId AllegianceId
    {
        get;
        private set;
    }

    public UnitId UnitIdDeathSpawn
    {
        get;
        private set;
    }

    public int DeathSpawnCount
    {
        get;
        private set;
    }

    public ZoneAoeId ZoneAoeIdDeathSpawn
    {
        get;
        private set;
    }

    public string SkinColor
    {
        get;
        private set;
    }

    public string FirstName
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (UnitId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Asset = reader.GetString(2);
        AltAsset = reader.GetString(3);
        Base = (reader.GetInt32(4) != 0);
        Released = (reader.GetInt32(5) != 0);
        RaceId = (RaceId)reader.GetInt32(6);
        WarbandId = (WarbandId)reader.GetInt32(7);
        MaxCount = reader.GetInt32(8);
        UnitTypeId = (UnitTypeId)reader.GetInt32(9);
        UnitSizeId = (UnitSizeId)reader.GetInt32(10);
        MovementId = (MovementId)reader.GetInt32(11);
        UnitWoundId = (UnitWoundId)reader.GetInt32(12);
        UnitBaseId = (UnitBaseId)reader.GetInt32(13);
        ItemIdTrophy = (ItemId)reader.GetInt32(14);
        AllegianceId = (AllegianceId)reader.GetInt32(15);
        UnitIdDeathSpawn = (UnitId)reader.GetInt32(16);
        DeathSpawnCount = reader.GetInt32(17);
        ZoneAoeIdDeathSpawn = (ZoneAoeId)reader.GetInt32(18);
        SkinColor = reader.GetString(19);
        FirstName = reader.GetString(20);
    }
}
