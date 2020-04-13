using RAIN.Core;
using UnityEngine;

public class AIShootClosest : AIShootBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "ShootClosest";
    }

    protected override bool ByPassLimit(UnitController current)
    {
        return false;
    }

    protected override int GetCriteriaValue(UnitController current)
    {
        return (int)Vector3.SqrMagnitude(current.transform.position - unitCtrlr.transform.position);
    }

    protected override bool IsBetter(int currentDist, int dist)
    {
        return currentDist < dist;
    }
}
