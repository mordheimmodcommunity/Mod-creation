using System.Collections.Generic;
using UnityEngine;

public class Fly : ICheapState
{
    private UnitController unitCtrlr;

    private FlyPoint bestPoint;

    public Fly(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        if (unitCtrlr.AICtrlr == null)
        {
            List<DecisionPoint> decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(unitCtrlr, DecisionPointId.FLY, float.MaxValue, excludeCloseToUnits: false);
            bestPoint = (FlyPoint)decisionPoints[0];
        }
        else
        {
            bestPoint = (FlyPoint)unitCtrlr.AICtrlr.targetDecisionPoint;
        }
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.SetKinemantic(kine: true);
        unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_fly"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/fly", cached: true));
        PandoraSingleton<MissionManager>.Instance.PlaySequence("fly", unitCtrlr, OnSeqDone);
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    public void FlyToPoint()
    {
        unitCtrlr.SetFixed(bestPoint.transform.position, fix: true);
        unitCtrlr.transform.rotation = bestPoint.transform.rotation;
        unitCtrlr.Imprint.alwaysHide = false;
        unitCtrlr.Imprint.needsRefresh = true;
    }

    public void OnSeqDone()
    {
        unitCtrlr.StateMachine.ChangeState(10);
    }
}
