using RAIN.Core;
using System.Collections.Generic;

public class AIPathWeakestEnemy : AIPathUnitBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathWeakestEnemy";
    }

    protected override bool CheckAllies()
    {
        return false;
    }

    protected override void SetTargets(List<UnitController> enemies)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (targets.Count == 0 || enemies[i].unit.CurrentWound < targets[0].unit.CurrentWound)
            {
                targets.Clear();
                targets.Add(enemies[i]);
            }
            else if (enemies[i].unit.CurrentWound == targets[0].unit.CurrentWound)
            {
                targets.Add(enemies[i]);
            }
        }
    }
}
