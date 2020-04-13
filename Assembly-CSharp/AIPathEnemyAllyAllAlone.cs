using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyAllyAllAlone : AIPathUnitBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathEnemyAllyAllAlone";
    }

    protected override bool CheckAllies()
    {
        return false;
    }

    protected override void SetTargets(List<UnitController> enemies)
    {
        int teamIdx = unitCtrlr.GetWarband().teamIdx;
        for (int i = 0; i < enemies.Count; i++)
        {
            for (int j = 0; j < enemies[i].EngagedUnits.Count; j++)
            {
                if (enemies[i].EngagedUnits[j].GetWarband().teamIdx == teamIdx && enemies[i].EngagedUnits[j].IsAllAlone())
                {
                    targets.Add(enemies[i]);
                }
            }
        }
    }
}
