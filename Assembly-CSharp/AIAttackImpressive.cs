using RAIN.Core;

public class AIAttackImpressive : AIAttackBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AttackImpressive";
    }

    protected override bool ByPassLimit(UnitController target)
    {
        return target.unit.Data.UnitTypeId == UnitTypeId.IMPRESSIVE;
    }

    protected override bool IsBetter(int currentVal, int val)
    {
        return false;
    }

    protected override int GetCriteriaValue(UnitController target)
    {
        return 0;
    }
}
