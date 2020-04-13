using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public abstract class AIPathSearchBase : AIBase
{
    private ActionResult currentResult;

    public override void Start(AI ai)
    {
        //IL_004b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d2: Unknown result type (might be due to invalid IL or missing references)
        base.Start(ai);
        base.actionName = "PathUnit";
        if (unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE || unitCtrlr.unit.BothArmsMutated())
        {
            success = false;
            currentResult = (ActionResult)2;
            return;
        }
        List<SearchPoint> targets = GetTargets();
        for (int num = targets.Count - 1; num >= 0; num--)
        {
            if (unitCtrlr.AICtrlr.AlreadyLootSearchPoint(targets[num]))
            {
                targets.RemoveAt(num);
            }
        }
        if (targets.Count > 0)
        {
            currentResult = (ActionResult)1;
            unitCtrlr.AICtrlr.FindPath(targets, OnSearchChecked);
        }
        else
        {
            success = false;
            currentResult = (ActionResult)2;
        }
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        return currentResult;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }

    public abstract List<SearchPoint> GetTargets();

    private void OnSearchChecked(bool success)
    {
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        success &= (unitCtrlr.AICtrlr.currentPath != null);
        base.success = success;
        currentResult = (ActionResult)((!success) ? 2 : 0);
    }
}
