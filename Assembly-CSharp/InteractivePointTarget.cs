using System.Collections.Generic;
using UnityEngine;

public class InteractivePointTarget : ICheapState
{
    private UnitController unitCtrlr;

    private List<InteractiveTarget> targets;

    private List<ActionStatus> actions;

    private CameraManager camRange;

    private int targetIdx;

    private int numActionZoneDestinations;

    public InteractivePointTarget(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
        targets = new List<InteractiveTarget>();
    }

    void ICheapState.Destroy()
    {
        targets.Clear();
    }

    void ICheapState.Enter(int iFrom)
    {
        camRange = PandoraSingleton<MissionManager>.Instance.CamManager;
        unitCtrlr.SetFixed(fix: true);
        targets.Clear();
        for (int i = 0; i < unitCtrlr.interactivePoints.Count; i++)
        {
            List<UnitActionId> unitActionIds = unitCtrlr.interactivePoints[i].GetUnitActionIds(unitCtrlr);
            for (int j = 0; j < unitActionIds.Count; j++)
            {
                List<ActionStatus> list = unitCtrlr.GetActions(unitActionIds[j]);
                for (int k = 0; k < list.Count; k++)
                {
                    if (list[k].Available)
                    {
                        targets.Add(new InteractiveTarget(list[k], unitCtrlr.interactivePoints[i]));
                    }
                }
            }
        }
        targets.Sort(new InteractiveTargetComparer());
        actions = new List<ActionStatus>();
        for (int l = 0; l < targets.Count; l++)
        {
            actions.Add(targets[l].action);
        }
        targetIdx = 0;
        UpdateDestination();
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_ZONE_CONFIRMED);
            unitCtrlr.SendInteractiveAction(unitCtrlr.CurrentAction.SkillId, targets[targetIdx].point);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cycling") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
        {
            UpdateDestination(1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("cycling") || PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
        {
            UpdateDestination(-1);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_ZONE_CANCEL);
            unitCtrlr.StateMachine.ChangeState(11);
        }
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].point.Highlight.On((i != targetIdx) ? Color.yellow : Color.red);
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void UpdateDestination(int move = 0)
    {
        targetIdx += move;
        if (targetIdx >= targets.Count)
        {
            targetIdx = 0;
        }
        else if (targetIdx < 0)
        {
            targetIdx = targets.Count - 1;
        }
        Transform transform = null;
        unitCtrlr.interactivePoint = targets[targetIdx].point;
        unitCtrlr.SetCurrentAction(targets[targetIdx].action.SkillId);
        unitCtrlr.prevInteractiveTarget = GetPrevInteractivePoint();
        unitCtrlr.nextInteractiveTarget = GetNextInteractivePoint();
        Transform transform2 = null;
        UnitActionId actionId = targets[targetIdx].action.ActionId;
        switch (actionId)
        {
            case UnitActionId.CLIMB:
                transform = ((ActionZone)unitCtrlr.interactivePoint).GetClimb().destination.transform;
                break;
            case UnitActionId.JUMP:
                transform = ((ActionZone)unitCtrlr.interactivePoint).GetJump().destination.transform;
                break;
            case UnitActionId.LEAP:
                transform = ((ActionZone)unitCtrlr.interactivePoint).GetLeap().destination.transform;
                break;
            case UnitActionId.SEARCH:
            case UnitActionId.ACTIVATE:
                transform = unitCtrlr.interactivePoint.transform;
                transform2 = ((!(unitCtrlr.interactivePoint.cameraAnchor != null)) ? null : unitCtrlr.interactivePoint.cameraAnchor.transform);
                break;
        }
        unitCtrlr.FaceTarget(transform.position);
        camRange.LookAtFocus(transform, overrideCurrentTarget: false);
        camRange.SwitchToCam(CameraManager.CameraType.CONSTRAINED, unitCtrlr.transform, transition: true, force: true, clearFocus: false, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        if (transform2 != null)
        {
            ConstrainedCamera currentCam = camRange.GetCurrentCam<ConstrainedCamera>();
            currentCam.SetOrigins(transform2);
        }
        camRange.AddLOSTarget(unitCtrlr.transform);
        switch (actionId)
        {
            case UnitActionId.CLIMB:
                unitCtrlr.activeActionDest = ((ActionZone)targets[targetIdx].point).GetClimb();
                break;
            case UnitActionId.JUMP:
                unitCtrlr.activeActionDest = ((ActionZone)targets[targetIdx].point).GetJump();
                break;
            case UnitActionId.LEAP:
                unitCtrlr.activeActionDest = ((ActionZone)targets[targetIdx].point).GetLeap();
                break;
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, targets[targetIdx].action, actions);
    }

    public InteractiveTarget GetPrevInteractivePoint()
    {
        if (targetIdx - 1 < 0)
        {
            return targets[targets.Count - 1];
        }
        return targets[targetIdx - 1];
    }

    public InteractiveTarget GetNextInteractivePoint()
    {
        if (targetIdx + 1 >= targets.Count)
        {
            return targets[0];
        }
        return targets[targetIdx + 1];
    }
}
