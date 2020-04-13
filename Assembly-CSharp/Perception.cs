using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class Perception : ICheapState
{
    private UnitController unitCtrlr;

    private CameraAnim camAnim;

    private bool perceptionSuccess;

    public Perception(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
        int perceptionRoll = unitCtrlr.unit.PerceptionRoll;
        perceptionSuccess = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, Mathf.Min(perceptionRoll, Constant.GetInt(ConstantId.MAX_ROLL)), AttributeId.PERCEPTION_ROLL);
        unitCtrlr.currentActionData.SetActionOutcome(perceptionSuccess);
        unitCtrlr.detectedUnits.Clear();
        unitCtrlr.detectedTriggers.Clear();
        unitCtrlr.detectedInteractivePoints.Clear();
        if (perceptionSuccess && unitCtrlr.IsPlayed())
        {
            unitCtrlr.detectedUnits.AddRange(PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(unitCtrlr.unit.warbandIdx));
            List<TriggerPoint> triggerPoints = PandoraSingleton<MissionManager>.Instance.triggerPoints;
            for (int i = 0; i < triggerPoints.Count; i++)
            {
                if (triggerPoints[i] is Trap)
                {
                    unitCtrlr.detectedTriggers.Add(triggerPoints[i]);
                }
            }
            List<InteractivePoint> interactivePoints = PandoraSingleton<MissionManager>.Instance.interactivePoints;
            for (int j = 0; j < interactivePoints.Count; j++)
            {
                if (interactivePoints[j] != null && interactivePoints[j].unitActionId == UnitActionId.SEARCH)
                {
                    unitCtrlr.detectedInteractivePoints.Add(interactivePoints[j]);
                }
            }
        }
        PandoraSingleton<MissionManager>.Instance.PlaySequence("perception", unitCtrlr, OnSeqDone);
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

    private void OnSeqDone()
    {
        unitCtrlr.StateMachine.ChangeState(10);
    }

    public void LaunchFx()
    {
        PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx((!perceptionSuccess) ? unitCtrlr.CurrentAction.fxData.FizzleFx : unitCtrlr.CurrentAction.fxData.LaunchFx, unitCtrlr, null, null);
    }
}
