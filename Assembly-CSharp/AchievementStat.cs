public class AchievementStat : Achievement
{
    public AchievementStat(AchievementData data)
        : base(data)
    {
    }

    private bool CheckStat(int value)
    {
        if (value >= base.Data.TargetStatValue)
        {
            return true;
        }
        return false;
    }

    public override bool CheckProfile(WarbandAttributeId statId, int value)
    {
        if (base.Data.AchievementTargetId == AchievementTargetId.PROFILE && base.Data.WarbandAttributeId != 0 && !base.Completed && base.Data.WarbandAttributeId == statId)
        {
            return CheckStat(value);
        }
        return false;
    }

    public override bool CheckWarband(Warband warband, WarbandAttributeId statId, int value)
    {
        if (base.Data.AchievementTargetId == AchievementTargetId.WARBAND && base.Data.WarbandAttributeId != 0 && !base.Completed && base.Data.WarbandAttributeId == statId)
        {
            return CheckStat(value);
        }
        return false;
    }

    public override bool CheckUnit(Unit unit, AttributeId statId, int value)
    {
        if (base.Data.AchievementTargetId == AchievementTargetId.UNIT && base.Data.AttributeId != 0 && !base.Completed && base.Data.AttributeId == statId && IsOfUnitType(unit, base.Data.UnitTypeId))
        {
            return CheckStat(value);
        }
        return false;
    }
}
