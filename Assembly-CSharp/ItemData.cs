using Mono.Data.Sqlite;

public class ItemData : DataCore
{
    public ItemId Id
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

    public ItemTypeId ItemTypeId
    {
        get;
        private set;
    }

    public bool Backup
    {
        get;
        private set;
    }

    public bool Paired
    {
        get;
        private set;
    }

    public bool Climbing
    {
        get;
        private set;
    }

    public bool Lootable
    {
        get;
        private set;
    }

    public bool Sellable
    {
        get;
        private set;
    }

    public bool LockSlot
    {
        get;
        private set;
    }

    public bool Stackable
    {
        get;
        private set;
    }

    public bool Undroppable
    {
        get;
        private set;
    }

    public bool MutationBased
    {
        get;
        private set;
    }

    public int PriceBuy
    {
        get;
        private set;
    }

    public int PriceSold
    {
        get;
        private set;
    }

    public int Rating
    {
        get;
        private set;
    }

    public int DamageMin
    {
        get;
        private set;
    }

    public int DamageMax
    {
        get;
        private set;
    }

    public int ArmorAbsorption
    {
        get;
        private set;
    }

    public ItemSpeedId ItemSpeedId
    {
        get;
        private set;
    }

    public ItemRangeId ItemRangeId
    {
        get;
        private set;
    }

    public TargetingId TargetingId
    {
        get;
        private set;
    }

    public AnimStyleId AnimStyleId
    {
        get;
        private set;
    }

    public BoneId BoneId
    {
        get;
        private set;
    }

    public ProjectileId ProjectileId
    {
        get;
        private set;
    }

    public int Shots
    {
        get;
        private set;
    }

    public int Radius
    {
        get;
        private set;
    }

    public string Sound
    {
        get;
        private set;
    }

    public string SoundCat
    {
        get;
        private set;
    }

    public bool TargetAlly
    {
        get;
        private set;
    }

    public bool IsIdol
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (ItemId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Asset = reader.GetString(2);
        ItemTypeId = (ItemTypeId)reader.GetInt32(3);
        Backup = (reader.GetInt32(4) != 0);
        Paired = (reader.GetInt32(5) != 0);
        Climbing = (reader.GetInt32(6) != 0);
        Lootable = (reader.GetInt32(7) != 0);
        Sellable = (reader.GetInt32(8) != 0);
        LockSlot = (reader.GetInt32(9) != 0);
        Stackable = (reader.GetInt32(10) != 0);
        Undroppable = (reader.GetInt32(11) != 0);
        MutationBased = (reader.GetInt32(12) != 0);
        PriceBuy = reader.GetInt32(13);
        PriceSold = reader.GetInt32(14);
        Rating = reader.GetInt32(15);
        DamageMin = reader.GetInt32(16);
        DamageMax = reader.GetInt32(17);
        ArmorAbsorption = reader.GetInt32(18);
        ItemSpeedId = (ItemSpeedId)reader.GetInt32(19);
        ItemRangeId = (ItemRangeId)reader.GetInt32(20);
        TargetingId = (TargetingId)reader.GetInt32(21);
        AnimStyleId = (AnimStyleId)reader.GetInt32(22);
        BoneId = (BoneId)reader.GetInt32(23);
        ProjectileId = (ProjectileId)reader.GetInt32(24);
        Shots = reader.GetInt32(25);
        Radius = reader.GetInt32(26);
        Sound = reader.GetString(27);
        SoundCat = reader.GetString(28);
        TargetAlly = (reader.GetInt32(29) != 0);
        IsIdol = (reader.GetInt32(30) != 0);
    }
}
