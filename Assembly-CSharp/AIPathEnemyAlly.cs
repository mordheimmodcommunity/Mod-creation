using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyAlly : AIPathUnitBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathEnemyAlly";
    }

    protected override bool CheckAllies()
    {
        return true;
    }

    protected override void SetTargets(List<UnitController> allies)
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i] != unitCtrlr && allies[i].Engaged)
            {
                targets.AddRange(allies[i].EngagedUnits);
            }
        }
    }
}
