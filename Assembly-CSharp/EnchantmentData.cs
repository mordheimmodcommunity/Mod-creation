using Mono.Data.Sqlite;

public class EnchantmentData : DataCore
{
    public EnchantmentId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public EffectTypeId EffectTypeId
    {
        get;
        private set;
    }

    public EnchantmentTypeId EnchantmentTypeId
    {
        get;
        private set;
    }

    public EnchantmentQualityId EnchantmentQualityId
    {
        get;
        private set;
    }

    public EnchantmentConsumeId EnchantmentConsumeId
    {
        get;
        private set;
    }

    public EnchantmentTriggerId EnchantmentTriggerIdDestroy
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

    public EnchantmentDmgTriggerId EnchantmentDmgTriggerId
    {
        get;
        private set;
    }

    public AttributeId AttributeIdDmgResistRoll
    {
        get;
        private set;
    }

    public int Duration
    {
        get;
        private set;
    }

    public bool ValidNextAction
    {
        get;
        private set;
    }

    public bool Indestructible
    {
        get;
        private set;
    }

    public bool RequireUnitState
    {
        get;
        private set;
    }

    public bool ChangeUnitState
    {
        get;
        private set;
    }

    public UnitStateId UnitStateIdRequired
    {
        get;
        private set;
    }

    public UnitStateId UnitStateIdNext
    {
        get;
        private set;
    }

    public bool Stackable
    {
        get;
        private set;
    }

    public bool DestroyOnApply
    {
        get;
        private set;
    }

    public bool KeepOnDeath
    {
        get;
        private set;
    }

    public bool NoDisplay
    {
        get;
        private set;
    }

    public bool MakeUnitVisible
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentIdOnTurnStart
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (EnchantmentId)reader.GetInt32(0);
        Name = reader.GetString(1);
        EffectTypeId = (EffectTypeId)reader.GetInt32(2);
        EnchantmentTypeId = (EnchantmentTypeId)reader.GetInt32(3);
        EnchantmentQualityId = (EnchantmentQualityId)reader.GetInt32(4);
        EnchantmentConsumeId = (EnchantmentConsumeId)reader.GetInt32(5);
        EnchantmentTriggerIdDestroy = (EnchantmentTriggerId)reader.GetInt32(6);
        DamageMin = reader.GetInt32(7);
        DamageMax = reader.GetInt32(8);
        EnchantmentDmgTriggerId = (EnchantmentDmgTriggerId)reader.GetInt32(9);
        AttributeIdDmgResistRoll = (AttributeId)reader.GetInt32(10);
        Duration = reader.GetInt32(11);
        ValidNextAction = (reader.GetInt32(12) != 0);
        Indestructible = (reader.GetInt32(13) != 0);
        RequireUnitState = (reader.GetInt32(14) != 0);
        ChangeUnitState = (reader.GetInt32(15) != 0);
        UnitStateIdRequired = (UnitStateId)reader.GetInt32(16);
        UnitStateIdNext = (UnitStateId)reader.GetInt32(17);
        Stackable = (reader.GetInt32(18) != 0);
        DestroyOnApply = (reader.GetInt32(19) != 0);
        KeepOnDeath = (reader.GetInt32(20) != 0);
        NoDisplay = (reader.GetInt32(21) != 0);
        MakeUnitVisible = (reader.GetInt32(22) != 0);
        EnchantmentIdOnTurnStart = (EnchantmentId)reader.GetInt32(23);
    }
}
