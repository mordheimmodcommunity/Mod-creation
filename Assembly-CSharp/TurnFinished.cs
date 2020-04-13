using UnityEngine;

public class TurnFinished : ICheapState
{
    private UnitController unitCtrlr;

    public TurnFinished(UnitController ctrler)
    {
        unitCtrlr = ctrler;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogInfo("TurnFinished Enter ", "UNIT_FLOW", unitCtrlr);
        if (unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(CombatLogger.LogMessage.TURN_END, unitCtrlr.GetLogName());
        }
        if (unitCtrlr.friendlyZoneEntryPoint != Vector3.zero)
        {
            unitCtrlr.transform.position = unitCtrlr.friendlyZoneEntryPoint;
            unitCtrlr.friendlyZoneEntryPoint = Vector3.zero;
        }
        if (unitCtrlr.AICtrlr != null)
        {
            unitCtrlr.AICtrlr.TurnEndCleanUp();
        }
        unitCtrlr.friendlyEntered.Clear();
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.SetKinemantic(kine: true);
        PandoraSingleton<MissionManager>.Instance.ClearBeacons();
        unitCtrlr.LastActivatedAction = null;
        unitCtrlr.unit.SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, 0);
        if (unitCtrlr.IsPlayed())
        {
            unitCtrlr.Imprint.SetCurrent(current: false);
            PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(unitCtrlr);
        }
        else
        {
            PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
        }
        unitCtrlr.SetAnimSpeed(0f);
        if (unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            unitCtrlr.SetGraphWalkability(walkable: false);
            unitCtrlr.GetComponent<Collider>().enabled = true;
        }
        else
        {
            unitCtrlr.SetGraphWalkability(walkable: true);
        }
        unitCtrlr.HideDetected();
        unitCtrlr.detectedUnits.Clear();
        unitCtrlr.detectedTriggers.Clear();
        unitCtrlr.detectedInteractivePoints.Clear();
        unitCtrlr.TurnStarted = false;
        unitCtrlr.beenShot = false;
        if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == unitCtrlr)
        {
            PandoraSingleton<MissionManager>.Instance.TurnTimer.Pause();
        }
        else
        {
            PandoraDebug.LogWarning("Ending unit turn when it's not the current unit " + unitCtrlr.name + "; current unit is: " + PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().name, "FLOW", unitCtrlr);
        }
        if (PandoraSingleton<Hermes>.Instance.IsConnected() && unitCtrlr.IsMine())
        {
            unitCtrlr.StopSync();
        }
        PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitCtrlr);
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitCtrlr);
        if (PandoraSingleton<GameManager>.Instance.currentSave != null && !PandoraSingleton<MissionManager>.Instance.MissionEndData.missionSave.isTuto)
        {
            PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(PandoraSingleton<GameManager>.Instance.currentSave, PandoraSingleton<GameManager>.Instance.campaign);
        }
    }

    void ICheapState.Update()
    {
        if (unitCtrlr.unit.IsAvailable())
        {
            if (unitCtrlr.unit.OverwatchLeft > 0 && unitCtrlr.HasRange())
            {
                PandoraDebug.LogInfo("Going to overwatch : (left " + unitCtrlr.unit.OverwatchLeft + ")", "ENDTURN", unitCtrlr);
                unitCtrlr.StateMachine.ChangeState(36);
                return;
            }
            if (unitCtrlr.unit.AmbushLeft > 0 && unitCtrlr.HasClose())
            {
                PandoraDebug.LogInfo("Going to ambush : (left " + unitCtrlr.unit.AmbushLeft + ")", "ENDTURN", unitCtrlr);
                unitCtrlr.StateMachine.ChangeState(37);
                return;
            }
        }
        unitCtrlr.StateMachine.ChangeState(9);
    }

    void ICheapState.FixedUpdate()
    {
    }
}
