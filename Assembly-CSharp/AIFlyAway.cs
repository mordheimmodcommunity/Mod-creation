using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIFlyAway : AIFlyCombat
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIFlyAway";
    }

    protected override void SetBestFlyPoint(List<DecisionPoint> flyPoints, List<UnitController> allAliveUnits)
    {
        int num = int.MaxValue;
        float num2 = 0f;
        for (int i = 0; i < flyPoints.Count; i++)
        {
            FlyPoint flyPoint = (FlyPoint)flyPoints[i];
            flyPoint.PointsChecker.UpdateControlPoints(unitCtrlr, allAliveUnits);
            if (flyPoint.PointsChecker.enemiesOnZone.Count <= num)
            {
                float num3 = Vector3.SqrMagnitude(unitCtrlr.transform.position - flyPoints[i].transform.position);
                if (num3 > num2)
                {
                    num = flyPoint.PointsChecker.enemiesOnZone.Count;
                    num2 = num3;
                    unitCtrlr.AICtrlr.targetDecisionPoint = flyPoint;
                }
            }
        }
    }
}
