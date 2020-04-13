using RAIN.Core;

public class AIAttackDefenseless : AIAttackBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIAttackDefenseless";
    }

    protected override bool ByPassLimit(UnitController target)
    {
        return !target.CanCounterAttack();
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
