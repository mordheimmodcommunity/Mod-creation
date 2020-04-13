using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIPathWyrdStoneBest : AIPathWyrdStoneClosest
{
    private RaycastHit hitInfo;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "PathWyrdStoneBest";
    }

    public override List<SearchPoint> GetTargets()
    {
        List<SearchPoint> targets = base.GetTargets();
        float num = unitCtrlr.unit.Movement;
        num *= num;
        int num2 = 0;
        for (int num3 = targets.Count - 1; num3 >= 0; num3--)
        {
            if (Vector3.SqrMagnitude(targets[num3].transform.position - unitCtrlr.transform.position) > num || !CanSeeSearchPoint(targets[num3]))
            {
                targets.RemoveAt(num3);
            }
            else if (targets[num3].items[0].PriceSold > num2)
            {
                num2 = targets[num3].items[0].PriceSold;
            }
        }
        for (int num4 = targets.Count - 1; num4 >= 0; num4--)
        {
            if (targets[num4].items[0].PriceSold < num2)
            {
                targets.RemoveAt(num4);
            }
        }
        return targets;
    }

    private bool CanSeeSearchPoint(SearchPoint target)
    {
        Vector3 position = unitCtrlr.transform.position;
        position.y += 1.5f;
        Vector3 position2 = target.transform.position;
        position2.y += 1.25f;
        float num = Vector3.SqrMagnitude(position - position2);
        Physics.Raycast(position, position2 - position, out hitInfo, unitCtrlr.unit.ViewDistance, LayerMaskManager.fowMask);
        if (hitInfo.collider == null || hitInfo.distance * hitInfo.distance > num)
        {
            return true;
        }
        return false;
    }
}
