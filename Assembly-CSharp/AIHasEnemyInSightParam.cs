using RAIN.Core;
using RAIN.Memory;
using RAIN.Representation;

public class AIHasEnemyInSightParam : AIBase
{
    public Expression distance;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "HasEnemyInSight";
        success = (unitCtrlr.HasEnemyInSight(distance.Evaluate<float>(0f, (RAINMemory)null)) || unitCtrlr.Engaged || unitCtrlr.AICtrlr.hasSeenEnemy);
    }
}
