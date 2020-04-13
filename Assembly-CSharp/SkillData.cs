using Mono.Data.Sqlite;

public class SkillData : DataCore
{
    public SkillId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public bool Released
    {
        get;
        private set;
    }

    public bool Passive
    {
        get;
        private set;
    }

    public SkillTypeId SkillTypeId
    {
        get;
        private set;
    }

    public SpellTypeId SpellTypeId
    {
        get;
        private set;
    }

    public EffectTypeId EffectTypeId
    {
        get;
        private set;
    }

    public UnitActionId UnitActionId
    {
        get;
        private set;
    }

    public BoneId BoneIdTarget
    {
        get;
        private set;
    }

    public SkillQualityId SkillQualityId
    {
        get;
        private set;
    }

    public SkillLineId SkillLineId
    {
        get;
        private set;
    }

    public SkillId SkillIdPrerequiste
    {
        get;
        private set;
    }

    public bool TargetSelf
    {
        get;
        private set;
    }

    public bool TargetAlly
    {
        get;
        private set;
    }

    public bool TargetEnemy
    {
        get;
        private set;
    }

    public TargetingId TargetingId
    {
        get;
        private set;
    }

    public bool NeedValidGround
    {
        get;
        private set;
    }

    public int StrategyPoints
    {
        get;
        private set;
    }

    public int OffensePoints
    {
        get;
        private set;
    }

    public int WoundsCostMin
    {
        get;
        private set;
    }

    public int WoundsCostMax
    {
        get;
        private set;
    }

    public AttributeId AttributeIdStat
    {
        get;
        private set;
    }

    public int StatValue
    {
        get;
        private set;
    }

    public int Cost
    {
        get;
        private set;
    }

    public int Time
    {
        get;
        private set;
    }

    public int Points
    {
        get;
        private set;
    }

    public bool NotEngaged
    {
        get;
        private set;
    }

    public bool Engaged
    {
        get;
        private set;
    }

    public bool NeedCloseSet
    {
        get;
        private set;
    }

    public bool NeedRangeSet
    {
        get;
        private set;
    }

    public bool WeaponLoaded
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentIdRequired
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentIdRequiredTarget
    {
        get;
        private set;
    }

    public int RangeMin
    {
        get;
        private set;
    }

    public int Range
    {
        get;
        private set;
    }

    public int Radius
    {
        get;
        private set;
    }

    public int Angle
    {
        get;
        private set;
    }

    public int WoundMin
    {
        get;
        private set;
    }

    public int WoundMax
    {
        get;
        private set;
    }

    public bool BypassArmor
    {
        get;
        private set;
    }

    public bool BypassMagicResist
    {
        get;
        private set;
    }

    public int LadderDiff
    {
        get;
        private set;
    }

    public ZoneAoeId ZoneAoeId
    {
        get;
        private set;
    }

    public TrapTypeId TrapTypeId
    {
        get;
        private set;
    }

    public DestructibleId DestructibleId
    {
        get;
        private set;
    }

    public bool AiProof
    {
        get;
        private set;
    }

    public bool AutoSuccess
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (SkillId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Released = (reader.GetInt32(2) != 0);
        Passive = (reader.GetInt32(3) != 0);
        SkillTypeId = (SkillTypeId)reader.GetInt32(4);
        SpellTypeId = (SpellTypeId)reader.GetInt32(5);
        EffectTypeId = (EffectTypeId)reader.GetInt32(6);
        UnitActionId = (UnitActionId)reader.GetInt32(7);
        BoneIdTarget = (BoneId)reader.GetInt32(8);
        SkillQualityId = (SkillQualityId)reader.GetInt32(9);
        SkillLineId = (SkillLineId)reader.GetInt32(10);
        SkillIdPrerequiste = (SkillId)reader.GetInt32(11);
        TargetSelf = (reader.GetInt32(12) != 0);
        TargetAlly = (reader.GetInt32(13) != 0);
        TargetEnemy = (reader.GetInt32(14) != 0);
        TargetingId = (TargetingId)reader.GetInt32(15);
        NeedValidGround = (reader.GetInt32(16) != 0);
        StrategyPoints = reader.GetInt32(17);
        OffensePoints = reader.GetInt32(18);
        WoundsCostMin = reader.GetInt32(19);
        WoundsCostMax = reader.GetInt32(20);
        AttributeIdStat = (AttributeId)reader.GetInt32(21);
        StatValue = reader.GetInt32(22);
        Cost = reader.GetInt32(23);
        Time = reader.GetInt32(24);
        Points = reader.GetInt32(25);
        NotEngaged = (reader.GetInt32(26) != 0);
        Engaged = (reader.GetInt32(27) != 0);
        NeedCloseSet = (reader.GetInt32(28) != 0);
        NeedRangeSet = (reader.GetInt32(29) != 0);
        WeaponLoaded = (reader.GetInt32(30) != 0);
        EnchantmentIdRequired = (EnchantmentId)reader.GetInt32(31);
        EnchantmentIdRequiredTarget = (EnchantmentId)reader.GetInt32(32);
        RangeMin = reader.GetInt32(33);
        Range = reader.GetInt32(34);
        Radius = reader.GetInt32(35);
        Angle = reader.GetInt32(36);
        WoundMin = reader.GetInt32(37);
        WoundMax = reader.GetInt32(38);
        BypassArmor = (reader.GetInt32(39) != 0);
        BypassMagicResist = (reader.GetInt32(40) != 0);
        LadderDiff = reader.GetInt32(41);
        ZoneAoeId = (ZoneAoeId)reader.GetInt32(42);
        TrapTypeId = (TrapTypeId)reader.GetInt32(43);
        DestructibleId = (DestructibleId)reader.GetInt32(44);
        AiProof = (reader.GetInt32(45) != 0);
        AutoSuccess = (reader.GetInt32(46) != 0);
    }
}
