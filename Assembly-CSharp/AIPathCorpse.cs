using RAIN.Core;
using System.Collections.Generic;

public class AIPathCorpse : AIPathSearchBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathCorpse";
    }

    public override List<SearchPoint> GetTargets()
    {
        int currentStrategyPoints = unitCtrlr.unit.CurrentStrategyPoints;
        currentStrategyPoints -= unitCtrlr.GetAction(SkillId.BASE_SEARCH).StrategyPoints;
        if (currentStrategyPoints > 0 && !unitCtrlr.unit.IsInventoryFull())
        {
            float dist = unitCtrlr.unit.Movement;
            List<SearchPoint> searchPointInRadius = PandoraSingleton<MissionManager>.Instance.GetSearchPointInRadius(unitCtrlr.transform.position, dist, UnitActionId.SEARCH);
            for (int num = searchPointInRadius.Count - 1; num >= 0; num--)
            {
                if (searchPointInRadius[num].unitController == null)
                {
                    searchPointInRadius.RemoveAt(num);
                }
            }
            return searchPointInRadius;
        }
        return new List<SearchPoint>();
    }
}
