using Mono.Data.Sqlite;

public class SkillAiFilterData : DataCore
{
    public SkillAiFilterId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public SkillId SkillId
    {
        get;
        private set;
    }

    public AiFilterResultId AiFilterResultId
    {
        get;
        private set;
    }

    public AttributeId AttributeId
    {
        get;
        private set;
    }

    public AiFilterCheckId AiFilterCheckId
    {
        get;
        private set;
    }

    public int CheckValue
    {
        get;
        private set;
    }

    public AttributeId AttributeIdCheck
    {
        get;
        private set;
    }

    public AiFilterCheckId AiFilterCheckIdEngaged
    {
        get;
        private set;
    }

    public int EngagedValue
    {
        get;
        private set;
    }

    public bool HasAltSet
    {
        get;
        private set;
    }

    public bool NeverUsedOnTarget
    {
        get;
        private set;
    }

    public bool NeverUsedTurn
    {
        get;
        private set;
    }

    public bool HasRangeWeapon
    {
        get;
        private set;
    }

    public bool IsAllAlone
    {
        get;
        private set;
    }

    public bool IsSister
    {
        get;
        private set;
    }

    public bool IsStunned
    {
        get;
        private set;
    }

    public bool CannotParry
    {
        get;
        private set;
    }

    public bool HasSpell
    {
        get;
        private set;
    }

    public int HealthUnderRatio
    {
        get;
        private set;
    }

    public int MinRoll
    {
        get;
        private set;
    }

    public bool HasBeenShot
    {
        get;
        private set;
    }

    public bool NoEnemyInSight
    {
        get;
        private set;
    }

    public bool IsPreFight
    {
        get;
        private set;
    }

    public EnchantmentTypeId EnchantmentTypeIdApplied
    {
        get;
        private set;
    }

    public SkillAiFilterId SkillAiFilterIdAnd
    {
        get;
        private set;
    }

    public bool CheckTargetInstead
    {
        get;
        private set;
    }

    public bool Reverse
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (SkillAiFilterId)reader.GetInt32(0);
        Name = reader.GetString(1);
        SkillId = (SkillId)reader.GetInt32(2);
        AiFilterResultId = (AiFilterResultId)reader.GetInt32(3);
        AttributeId = (AttributeId)reader.GetInt32(4);
        AiFilterCheckId = (AiFilterCheckId)reader.GetInt32(5);
        CheckValue = reader.GetInt32(6);
        AttributeIdCheck = (AttributeId)reader.GetInt32(7);
        AiFilterCheckIdEngaged = (AiFilterCheckId)reader.GetInt32(8);
        EngagedValue = reader.GetInt32(9);
        HasAltSet = (reader.GetInt32(10) != 0);
        NeverUsedOnTarget = (reader.GetInt32(11) != 0);
        NeverUsedTurn = (reader.GetInt32(12) != 0);
        HasRangeWeapon = (reader.GetInt32(13) != 0);
        IsAllAlone = (reader.GetInt32(14) != 0);
        IsSister = (reader.GetInt32(15) != 0);
        IsStunned = (reader.GetInt32(16) != 0);
        CannotParry = (reader.GetInt32(17) != 0);
        HasSpell = (reader.GetInt32(18) != 0);
        HealthUnderRatio = reader.GetInt32(19);
        MinRoll = reader.GetInt32(20);
        HasBeenShot = (reader.GetInt32(21) != 0);
        NoEnemyInSight = (reader.GetInt32(22) != 0);
        IsPreFight = (reader.GetInt32(23) != 0);
        EnchantmentTypeIdApplied = (EnchantmentTypeId)reader.GetInt32(24);
        SkillAiFilterIdAnd = (SkillAiFilterId)reader.GetInt32(25);
        CheckTargetInstead = (reader.GetInt32(26) != 0);
        Reverse = (reader.GetInt32(27) != 0);
    }
}
