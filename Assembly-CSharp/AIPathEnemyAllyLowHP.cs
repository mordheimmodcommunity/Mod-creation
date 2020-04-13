using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyAllyLowHP : AIPathUnitBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathEnemyAllyLowHP";
    }

    protected override bool CheckAllies()
    {
        return true;
    }

    protected override void SetTargets(List<UnitController> enemies)
    {
        int teamIdx = unitCtrlr.GetWarband().teamIdx;
        for (int i = 0; i < enemies.Count; i++)
        {
            for (int j = 0; j < enemies[i].EngagedUnits.Count; j++)
            {
                if (enemies[i].EngagedUnits[j].GetWarband().teamIdx == teamIdx && !enemies[i].EngagedUnits[j].CanDisengage())
                {
                    targets.Add(enemies[i]);
                }
                if (enemies[i].EngagedUnits[j].GetWarband().teamIdx == teamIdx && enemies[i].unit.IsLeader)
                {
                    targets.Add(enemies[i]);
                }
            }
        }
    }
}
