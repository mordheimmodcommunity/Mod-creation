using System.Collections.Generic;

public class CounterChoice : ICheapState
{
    private UnitController unitCtrlr;

    private TurnTimer timer;

    public CounterChoice(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.defenderCtrlr = unitCtrlr.attackerCtrlr;
        unitCtrlr.SetCurrentAction(SkillId.BASE_COUNTER_ATTACK);
        timer = null;
        if (unitCtrlr.IsPlayed() && PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.turnTimer != 0)
        {
            timer = new TurnTimer(Constant.GetInt(ConstantId.COUNTER_TIMER), OnTimerDone);
            timer.Reset();
            timer.Resume();
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, unitCtrlr);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_TARGET_CHANGED, unitCtrlr.defenderCtrlr);
        PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_SINGLE_TARGETING);
        }
    }

    void ICheapState.Exit(int iTo)
    {
        if (timer != null)
        {
            timer.Pause();
        }
    }

    void ICheapState.Update()
    {
        if (timer != null)
        {
            timer.Update();
        }
        if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && (unitCtrlr.AICtrlr != null || unitCtrlr.unit.CounterForced > 0))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CLOSE_COMBAT_COUNTER_ATTACK_VALID);
            unitCtrlr.SendSkillSingleTarget(SkillId.BASE_COUNTER_ATTACK, unitCtrlr.attackerCtrlr);
        }
        else if (unitCtrlr.IsPlayed())
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action") || unitCtrlr.unit.CounterForced > 0)
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CLOSE_COMBAT_COUNTER_ATTACK_VALID);
                unitCtrlr.SendSkillSingleTarget(SkillId.BASE_COUNTER_ATTACK, unitCtrlr.attackerCtrlr);
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
            {
                unitCtrlr.SendActionDone();
            }
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void OnTimerDone()
    {
        if (unitCtrlr.IsPlayed())
        {
            unitCtrlr.SendActionDone();
        }
    }
}
