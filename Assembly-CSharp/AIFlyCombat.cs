using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIFlyCombat : AIBase
{
    private const float MIN_VALID_SQR_DIST = 25f;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIFlyCombat";
        ActionStatus action = unitCtrlr.GetAction(SkillId.BASE_FLY);
        action.UpdateAvailable();
        success = action.Available;
        if (!success)
        {
            return;
        }
        unitCtrlr.AICtrlr.targetDecisionPoint = null;
        List<DecisionPoint> decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(unitCtrlr, DecisionPointId.FLY, float.MaxValue, excludeCloseToUnits: false);
        List<UnitController> allAliveUnits = PandoraSingleton<MissionManager>.Instance.GetAllAliveUnits();
        for (int num = decisionPoints.Count - 1; num >= 0; num--)
        {
            if (Vector3.SqrMagnitude(unitCtrlr.transform.position - decisionPoints[num].transform.position) < 25f)
            {
                decisionPoints.RemoveAt(num);
            }
        }
        SetBestFlyPoint(decisionPoints, allAliveUnits);
        success = (unitCtrlr.AICtrlr.targetDecisionPoint != null);
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        if (success)
        {
            unitCtrlr.SendSkill(SkillId.BASE_FLY);
        }
        return base.Execute(ai);
    }

    protected virtual void SetBestFlyPoint(List<DecisionPoint> flyPoints, List<UnitController> allAliveUnits)
    {
        int count = unitCtrlr.EngagedUnits.Count;
        for (int i = 0; i < flyPoints.Count; i++)
        {
            FlyPoint flyPoint = (FlyPoint)flyPoints[i];
            flyPoint.PointsChecker.UpdateControlPoints(unitCtrlr, allAliveUnits);
            if (flyPoint.PointsChecker.enemiesOnZone.Count > count)
            {
                count = flyPoint.PointsChecker.enemiesOnZone.Count;
                unitCtrlr.AICtrlr.targetDecisionPoint = flyPoint;
            }
        }
        if (!(unitCtrlr.AICtrlr.targetDecisionPoint == null) || unitCtrlr.Engaged)
        {
            return;
        }
        float num = float.MaxValue;
        List<UnitController> aliveEnemies = PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(unitCtrlr.GetWarband().idx);
        for (int j = 0; j < flyPoints.Count; j++)
        {
            for (int k = 0; k < aliveEnemies.Count; k++)
            {
                float num2 = Vector3.SqrMagnitude(flyPoints[j].transform.position - aliveEnemies[k].transform.position);
                if (num2 < num)
                {
                    num = num2;
                    unitCtrlr.AICtrlr.targetDecisionPoint = flyPoints[j];
                }
            }
        }
    }
}
