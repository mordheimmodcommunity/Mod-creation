using UnityEngine;

public class Rout : ICheapState
{
    private MissionManager missionMngr;

    private bool routSuccess;

    private UnitController rollUnit;

    private WarbandController warCtrlr;

    public Rout(MissionManager mngr)
    {
        missionMngr = mngr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        warCtrlr = missionMngr.GetCurrentUnit().GetWarband();
        PandoraDebug.LogDebug("WarbandIdx = " + warCtrlr.idx + " Ratio = " + warCtrlr.MoralRatio + "Rout at = " + PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold, "ROUT");
        if (missionMngr.GetCurrentUnit().unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            missionMngr.CombatLogger.AddLog(CombatLogger.LogMessage.TURN_START, missionMngr.GetCurrentUnit().GetLogName());
            if (warCtrlr.defeated || !warCtrlr.canRout || warCtrlr.MoralValue == warCtrlr.OldMoralValue)
            {
                Done();
            }
            else if (warCtrlr.MoralRatio >= PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
            {
                Done();
            }
            else if (warCtrlr.MoralValue <= 0)
            {
                warCtrlr.defeated = true;
                Done();
            }
            else if (warCtrlr.MoralRatio < PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
            {
                missionMngr.CombatLogger.AddLog(CombatLogger.LogMessage.MORAL_BELOW, warCtrlr.MoralValue.ToString(), warCtrlr.MaxMoralValue.ToString(), Mathf.FloorToInt(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold * 100f).ToString());
                RoutRoll();
            }
            else
            {
                Done();
            }
        }
        else
        {
            Done();
        }
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

    private void CheckWarband()
    {
    }

    private void RoutRoll()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.PHASE_ROUT);
        warCtrlr.OldMoralValue = warCtrlr.MoralValue;
        rollUnit = warCtrlr.GetAliveLeader();
        if (rollUnit == null)
        {
            warCtrlr.defeated = true;
            rollUnit = warCtrlr.GetLeader();
        }
        if (rollUnit == null)
        {
            rollUnit = warCtrlr.unitCtrlrs.Find((UnitController x) => x.unit.Status != UnitStateId.OUT_OF_ACTION);
        }
        if (rollUnit == null)
        {
            rollUnit = warCtrlr.unitCtrlrs[0];
        }
        rollUnit.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_rout_roll"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/rout", cached: true));
        int num = rollUnit.unit.WarbandRoutRoll;
        if (!rollUnit.unit.IsAvailable() || warCtrlr.MoralValue == 0 || warCtrlr.AllUnitsDead())
        {
            num = 0;
        }
        rollUnit.recoveryTarget = num;
        routSuccess = rollUnit.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, num, AttributeId.LEADERSHIP_ROLL);
        warCtrlr.defeated = !routSuccess;
        PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog((!routSuccess) ? CombatLogger.LogMessage.ROUT_FAIL : CombatLogger.LogMessage.ROUT_SUCCESS);
        rollUnit.currentActionData.SetActionOutcome(routSuccess);
        PandoraSingleton<MissionManager>.Instance.PlaySequence("moral_check", rollUnit, Done);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_ROUT_TEST, warCtrlr, routSuccess);
    }

    private void Done()
    {
        if (!missionMngr.CheckEndGame())
        {
            missionMngr.GetCurrentUnit().nextState = UnitController.State.TURN_START;
            missionMngr.WatchOrMove();
        }
    }
}
