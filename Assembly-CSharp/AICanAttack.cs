using RAIN.Core;

public class AICanAttack : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "CanAttack";
        success = ((unitCtrlr.HasClose() || unitCtrlr.IsAltClose()) && unitCtrlr.HasEnemyInSight());
    }
}
