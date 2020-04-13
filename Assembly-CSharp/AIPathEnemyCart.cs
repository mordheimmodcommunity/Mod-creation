using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyCart : AIPathSearchBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathEnemyCart";
    }

    public override List<SearchPoint> GetTargets()
    {
        List<SearchPoint> list = new List<SearchPoint>();
        int teamIdx = unitCtrlr.GetWarband().teamIdx;
        foreach (WarbandController warbandCtrlr in PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs)
        {
            if (warbandCtrlr.teamIdx != teamIdx)
            {
                list.Add(warbandCtrlr.wagon.chest);
            }
        }
        return list;
    }
}
