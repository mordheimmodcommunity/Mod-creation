using System.Collections.Generic;
using UnityEngine;

public class SingleTargeting : ICheapState
{
    private UnitController unitCtrlr;

    private CameraManager camRange;

    private int currentTargetIdx;

    private MonoBehaviour lastTarget;

    private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

    public SingleTargeting(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        camRange = PandoraSingleton<MissionManager>.Instance.CamManager;
        availableTargets = unitCtrlr.GetCurrentActionTargets();
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.defenderCtrlr = null;
        if (lastTarget != null && availableTargets.IndexOf(lastTarget) != -1)
        {
            currentTargetIdx = availableTargets.IndexOf(lastTarget);
            UpdateTarget();
        }
        else
        {
            currentTargetIdx = -1;
            UpdateTarget(1);
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_SINGLE_TARGETING);
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
        }
    }

    void ICheapState.Exit(int iTo)
    {
        currentTargetIdx = -1;
        availableTargets.Clear();
        camRange.ClearLookAtFocus();
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CANCEL);
            unitCtrlr.StateMachine.ChangeState((!unitCtrlr.Engaged) ? 11 : 12);
            return;
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
            if (unitCtrlr.defenderCtrlr != null)
            {
                unitCtrlr.SendSkillSingleTarget(unitCtrlr.CurrentAction.SkillId, unitCtrlr.defenderCtrlr);
            }
            else
            {
                unitCtrlr.SendSkillSingleDestructible(unitCtrlr.CurrentAction.SkillId, unitCtrlr.destructibleTarget);
            }
            return;
        }
        for (int i = 0; i < availableTargets.Count; i++)
        {
            if (availableTargets[i] is UnitController)
            {
                ((UnitController)availableTargets[i]).Highlight.On((i != currentTargetIdx) ? Color.yellow : Color.red);
            }
            else if (availableTargets[i] is Destructible)
            {
                ((Destructible)availableTargets[i]).Highlight.On((i != currentTargetIdx) ? Color.yellow : Color.red);
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cycling") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
        {
            UpdateTarget(1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("cycling") || PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
        {
            UpdateTarget(-1);
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void UpdateTarget(int move = 0)
    {
        int num = currentTargetIdx;
        currentTargetIdx += move;
        currentTargetIdx = ((currentTargetIdx < availableTargets.Count) ? ((currentTargetIdx >= 0) ? currentTargetIdx : (availableTargets.Count - 1)) : 0);
        if (currentTargetIdx == num && move != 0)
        {
            return;
        }
        MonoBehaviour monoBehaviour = lastTarget = availableTargets[currentTargetIdx];
        if (unitCtrlr != unitCtrlr.defenderCtrlr)
        {
            unitCtrlr.FaceTarget(monoBehaviour.transform);
        }
        camRange.LookAtFocus(monoBehaviour.transform, overrideCurrentTarget: false);
        PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.SEMI_CONSTRAINED, unitCtrlr.transform, transition: true, force: true, clearFocus: false, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        for (int i = 0; i < availableTargets.Count; i++)
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(availableTargets[i].transform);
        }
        if (monoBehaviour is UnitController)
        {
            unitCtrlr.defenderCtrlr = (UnitController)monoBehaviour;
            unitCtrlr.destructibleTarget = null;
            if (unitCtrlr.IsPlayed())
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_TARGET_CHANGED, unitCtrlr.defenderCtrlr);
            }
        }
        else if (monoBehaviour is Destructible)
        {
            unitCtrlr.defenderCtrlr = null;
            unitCtrlr.destructibleTarget = (Destructible)monoBehaviour;
            unitCtrlr.currentSpellTargetPosition = monoBehaviour.transform.position;
            if (unitCtrlr.IsPlayed())
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_TARGET_DESTUCTIBLE_CHANGED, unitCtrlr.destructibleTarget);
            }
        }
    }
}
