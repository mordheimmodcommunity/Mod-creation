using System.Collections.Generic;

public class Engaged : ICheapState
{
    private UnitController unitCtrlr;

    public int actionIndex;

    public Engaged(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    public void Destroy()
    {
        unitCtrlr = null;
    }

    public void Enter(int iFrom)
    {
        unitCtrlr.interactivePoint = null;
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.UpdateActionStatus(notice: true);
        unitCtrlr.wasEngaged = true;
        UpdateCurrentAction();
        if (!unitCtrlr.EngagedUnits.Contains(unitCtrlr.defenderCtrlr))
        {
            unitCtrlr.defenderCtrlr = unitCtrlr.EngagedUnits[0];
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UPDATE_TARGET, unitCtrlr.defenderCtrlr, unitCtrlr.defenderCtrlr.unit.warbandIdx);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_STATUS_ENGAGE, v1: true);
        PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitCtrlr.transform, transition: true, force: false, clearFocus: true, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        PandoraSingleton<MissionManager>.Instance.ShowCombatCircles(unitCtrlr);
        unitCtrlr.Send(true, Hermes.SendTarget.OTHERS, unitCtrlr.uid, 1u, 0f, unitCtrlr.transform.rotation, unitCtrlr.transform.position);
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_MOVE);
        }
    }

    public void Exit(int iTo)
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_STATUS_ENGAGE, v1: false);
        unitCtrlr.lastActionWounds = 0;
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION_CLEAR);
    }

    public void FixedUpdate()
    {
    }

    public void Update()
    {
        if (unitCtrlr.CurrentAction.waitForConfirmation)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
            {
                unitCtrlr.CurrentAction.Select();
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel"))
            {
                unitCtrlr.CurrentAction.Cancel();
            }
            return;
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview"))
        {
            unitCtrlr.StateMachine.ChangeState(44);
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel"))
        {
            unitCtrlr.SetCurrentAction(SkillId.BASE_END_TURN);
            unitCtrlr.CurrentAction.Select();
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            unitCtrlr.CurrentAction.Select();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cycling"))
        {
            UpdateCurrentAction(1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("cycling"))
        {
            UpdateCurrentAction(-1);
        }
    }

    private void UpdateCurrentAction(int dir = 0)
    {
        if (unitCtrlr.availableActionStatus.Count > 0)
        {
            actionIndex += dir;
            actionIndex = ((actionIndex < unitCtrlr.availableActionStatus.Count) ? ((actionIndex >= 0) ? actionIndex : (unitCtrlr.availableActionStatus.Count - 1)) : 0);
            unitCtrlr.SetCurrentAction(unitCtrlr.availableActionStatus[actionIndex].SkillId);
            if (unitCtrlr.IsPlayed())
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction);
            }
        }
        else if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, null);
        }
    }
}
