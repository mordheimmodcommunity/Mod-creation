using System.Collections.Generic;
using UnityEngine;

public class Moving : ICheapState
{
    private const float turnSmoothing = 7f;

    private const float speedDampTime = 0.1f;

    private static readonly ActionStatusComparer ActionStatusComparer = new ActionStatusComparer();

    private UnitController unitCtrlr;

    private Rigidbody rigBody;

    private Vector3 lastValidPosition;

    public List<ActionStatus> actions = new List<ActionStatus>();

    private ActionStatus interactionAction;

    private List<ActionStatus> possibleActions = new List<ActionStatus>();

    public int actionIndex;

    private int oldTempStrats;

    private bool enchantRemoved;

    private bool zonesSet;

    private bool alliesCutterReduced;

    private bool uiDisplayed;

    public Moving(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
        rigBody = unitCtrlr.GetComponent<Rigidbody>();
        actionIndex = 0;
        SkillData data = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>(455);
        interactionAction = new ActionStatus(data, unitCtrlr);
        interactionAction.UpdateAvailable();
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        zonesSet = false;
        alliesCutterReduced = false;
        unitCtrlr.SetFixed(fix: false);
        unitCtrlr.interactivePoint = null;
        UpdateInteractivePoints();
        PandoraSingleton<MissionManager>.Instance.SetAccessibleActionZones(unitCtrlr, delegate
        {
            zonesSet = true;
        });
        float num = unitCtrlr.unit.Movement * unitCtrlr.unit.Movement;
        float num2 = PandoraUtils.FlatSqrDistance(unitCtrlr.startPosition, unitCtrlr.transform.position);
        if (num2 <= num)
        {
            lastValidPosition = unitCtrlr.transform.position;
        }
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.pointingArrows.Count; i++)
        {
            GameObject gameObject = PandoraSingleton<MissionManager>.Instance.pointingArrows[i];
            gameObject.transform.SetParent(unitCtrlr.transform);
            gameObject.transform.localPosition = Vector3.zero;
        }
        PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitCtrlr.transform, transition: true, force: false, clearFocus: true, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        PandoraSingleton<MissionManager>.Instance.ShowCombatCircles(unitCtrlr);
        unitCtrlr.defenderCtrlr = null;
        PandoraDebug.LogDebug("Moving Enter - wasEngaged = " + unitCtrlr.wasEngaged + " IsEngaged Now = " + unitCtrlr.Engaged, "FLOW", unitCtrlr);
        PandoraSingleton<MissionManager>.Instance.RefreshActionZones(unitCtrlr);
        RefreshActions(UnitActionRefreshId.ALWAYS);
        if (unitCtrlr.CurrentAction.waitForConfirmation)
        {
            actionIndex = actions.IndexOf(unitCtrlr.CurrentAction);
        }
        UpdateCurrentAction();
        oldTempStrats = unitCtrlr.unit.tempStrategyPoints;
        if (!unitCtrlr.Engaged && PandoraSingleton<MissionManager>.Instance.ActiveBeacons() == 0 && unitCtrlr.unit.CurrentStrategyPoints > 0)
        {
            unitCtrlr.SpawnBeacon();
        }
        else
        {
            RefreshActions(UnitActionRefreshId.ALWAYS);
            if (unitCtrlr.unit.CurrentStrategyPoints > 0)
            {
                PandoraSingleton<MissionManager>.Instance.MoveCircle.Show(unitCtrlr.startPosition, unitCtrlr.unit.Movement);
            }
        }
        unitCtrlr.ReduceAlliesNavCutterSize(delegate
        {
            alliesCutterReduced = true;
        });
        PandoraDebug.LogInfo("Moving connected : " + PandoraSingleton<Hermes>.Instance.IsConnected(), "HERMES");
        PandoraDebug.LogInfo("Moving  isMine : " + unitCtrlr.IsMine(), "HERMES");
        PandoraDebug.LogDebug("start sync for unit ID " + unitCtrlr.uid + " Owner ID =" + unitCtrlr.owner);
        uiDisplayed = false;
        PandoraSingleton<UIMissionManager>.Instance.unitAction.OnDisable();
        enchantRemoved = false;
    }

    void ICheapState.Exit(int iTo)
    {
        unitCtrlr.newRotation = Quaternion.identity;
        unitCtrlr.lastActionWounds = 0;
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION_CLEAR);
        if (iTo != 44)
        {
            unitCtrlr.RestoreAlliesNavCutterSize();
            if (!unitCtrlr.wasEngaged)
            {
                unitCtrlr.ClampToNavMesh();
            }
        }
        unitCtrlr.HideDetected();
        unitCtrlr.SetAnimSpeed(0f);
        PandoraSingleton<MissionManager>.Instance.TurnOffActionZones();
        HidePointingArrows();
        PandoraSingleton<MissionManager>.Instance.MoveCircle.Hide();
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.COMBAT_HIGHLIGHT_TARGET, -1, new List<UnitController>());
    }

    void ICheapState.Update()
    {
        if (!zonesSet || !alliesCutterReduced)
        {
            return;
        }
        if (!uiDisplayed)
        {
            uiDisplayed = true;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_MOVE);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INTERACTION_POINTS_CHANGED);
        }
        if (unitCtrlr.CurrentAction.waitForConfirmation)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
            {
                unitCtrlr.CurrentAction.Select();
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
            {
                unitCtrlr.CurrentAction.Cancel();
            }
            return;
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview"))
        {
            unitCtrlr.StateMachine.ChangeState(44);
        }
        MovementCircles moveCircle = PandoraSingleton<MissionManager>.Instance.MoveCircle;
        Vector3 position = unitCtrlr.transform.position;
        moveCircle.AdjustHeightAndRadius(position.y, unitCtrlr.unit.GetAttribute(AttributeId.MOVEMENT));
        if (!unitCtrlr.Engaged)
        {
            unitCtrlr.RefreshDetected();
        }
        if (unitCtrlr.Engaged)
        {
            return;
        }
        Vector3 deltaPosition = unitCtrlr.animator.deltaPosition;
        if (deltaPosition.x != 0f)
        {
            return;
        }
        Vector3 deltaPosition2 = unitCtrlr.animator.deltaPosition;
        if (deltaPosition2.z == 0f && !unitCtrlr.IsAnimating() && unitCtrlr.animator.GetInteger(AnimatorIds.action) == 0)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") && unitCtrlr.GetAction(SkillId.BASE_END_TURN).Available)
            {
                actionIndex = actions.IndexOf(unitCtrlr.GetAction(SkillId.BASE_END_TURN));
                unitCtrlr.SetCurrentAction(SkillId.BASE_END_TURN);
                unitCtrlr.CurrentAction.Select();
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, actions);
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
    }

    void ICheapState.FixedUpdate()
    {
        if (!zonesSet || !alliesCutterReduced || unitCtrlr.CurrentAction.waitForConfirmation)
        {
            return;
        }
        unitCtrlr.CheckEngaged(applyEnchants: true);
        if (unitCtrlr.Engaged)
        {
            unitCtrlr.SendEngaged(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
            return;
        }
        unitCtrlr.ClampToNavMesh();
        RefreshActions(UnitActionRefreshId.ALWAYS);
        ShowPointingArrows();
        float num = PandoraUtils.FlatSqrDistance(unitCtrlr.startPosition, unitCtrlr.transform.position);
        float num2 = Vector3.SqrMagnitude(unitCtrlr.startPosition - unitCtrlr.transform.position);
        float num3 = unitCtrlr.unit.Movement * unitCtrlr.unit.Movement;
        if (num > num3)
        {
            if (unitCtrlr.unit.Movement > 0 && unitCtrlr.unit.tempStrategyPoints < unitCtrlr.unit.CurrentStrategyPoints)
            {
                unitCtrlr.SpawnBeacon();
                UpdateCurrentAction();
                num = PandoraUtils.FlatSqrDistance(unitCtrlr.startPosition, unitCtrlr.transform.position);
            }
            else
            {
                unitCtrlr.transform.position = lastValidPosition;
                if (!rigBody.isKinematic)
                {
                    rigBody.velocity = Vector3.zero;
                }
            }
        }
        else
        {
            lastValidPosition = unitCtrlr.transform.position;
        }
        if (unitCtrlr.CurrentBeacon != null)
        {
            unitCtrlr.CurrentBeacon.SetActive(num2 > 1f);
        }
        if (num2 > 1f && !enchantRemoved)
        {
            unitCtrlr.unit.RemoveEnchantments(EnchantmentId.CLIMB_FAIL_EFFECT);
            enchantRemoved = true;
        }
        unitCtrlr.unit.tempStrategyPoints = Mathf.Clamp(PandoraSingleton<MissionManager>.Instance.ActiveBeacons() - 1 + ((num2 > 1f) ? 1 : 0), 0, unitCtrlr.unit.StrategyPoints);
        if (oldTempStrats != unitCtrlr.unit.tempStrategyPoints)
        {
            oldTempStrats = unitCtrlr.unit.tempStrategyPoints;
            RefreshActions(UnitActionRefreshId.NONE);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_ATTRIBUTES_CHANGED, unitCtrlr.unit);
        }
        float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("v");
        float axis2 = PandoraSingleton<PandoraInput>.Instance.GetAxis("h");
        if (unitCtrlr.unit.CurrentStrategyPoints > 0 && (axis2 != 0f || axis != 0f) && unitCtrlr.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == AnimatorIds.idle)
        {
            Vector3 forward = PandoraSingleton<MissionManager>.Instance.CamManager.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            forward *= axis;
            Vector3 right = PandoraSingleton<MissionManager>.Instance.CamManager.transform.right;
            right.y = 0f;
            right.Normalize();
            right *= axis2;
            Vector3 forward2 = forward + right;
            Quaternion b = Quaternion.LookRotation(forward2, Vector3.up);
            unitCtrlr.newRotation = Quaternion.Lerp(unitCtrlr.transform.rotation, b, 7f * Time.fixedDeltaTime);
            unitCtrlr.animator.SetFloat(AnimatorIds.speed, forward2.magnitude, 0.1f, Time.fixedDeltaTime);
            unitCtrlr.SetFixed(fix: false);
            PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(unitCtrlr);
            PandoraSingleton<MissionManager>.Instance.RefreshActionZones(unitCtrlr);
            PandoraSingleton<MissionManager>.Instance.UpdateCombatCirclesAlpha(unitCtrlr);
        }
        else
        {
            if (unitCtrlr.animator.GetFloat(AnimatorIds.speed) > 0f)
            {
                unitCtrlr.SetAnimSpeed(0f);
            }
            unitCtrlr.newRotation = Quaternion.identity;
            unitCtrlr.GetComponent<Rigidbody>().drag = 100f;
        }
    }

    private void RefreshActions(UnitActionRefreshId actionRefreshId)
    {
        if (unitCtrlr.UpdateActionStatus(notice: false, actionRefreshId))
        {
            UpdateCurrentAction();
        }
    }

    private void UpdateCurrentAction(int dir = 0)
    {
        actions.Clear();
        for (int i = 0; i < unitCtrlr.availableActionStatus.Count; i++)
        {
            if (!unitCtrlr.availableActionStatus[i].actionData.Interactive)
            {
                actions.Add(unitCtrlr.availableActionStatus[i]);
            }
        }
        ActionStatus actionStatus = null;
        InteractivePoint interactivePoint = null;
        possibleActions.Clear();
        for (int j = 0; j < unitCtrlr.interactivePoints.Count; j++)
        {
            InteractivePoint interactivePoint2 = unitCtrlr.interactivePoints[j];
            List<UnitActionId> unitActionIds = interactivePoint2.GetUnitActionIds(unitCtrlr);
            for (int k = 0; k < unitActionIds.Count; k++)
            {
                UnitActionId unitActionId = unitActionIds[k];
                for (int l = 0; l < unitCtrlr.actionStatus.Count; l++)
                {
                    if (unitCtrlr.actionStatus[l].ActionId == unitActionId && unitCtrlr.actionStatus[l].Available)
                    {
                        interactivePoint = interactivePoint2;
                        possibleActions.Add(unitCtrlr.actionStatus[l]);
                    }
                }
                if (possibleActions.Count > 1)
                {
                    break;
                }
            }
        }
        if (possibleActions.Count > 0)
        {
            actionStatus = ((actionStatus != null || possibleActions.Count != 1) ? interactionAction : possibleActions[0]);
        }
        if (actionStatus != null)
        {
            actions.Add(actionStatus);
            if (actionStatus != interactionAction)
            {
                unitCtrlr.interactivePoint = interactivePoint;
                switch (actionStatus.ActionId)
                {
                    case UnitActionId.CLIMB:
                        unitCtrlr.activeActionDest = ((ActionZone)interactivePoint).GetClimb();
                        break;
                    case UnitActionId.JUMP:
                        unitCtrlr.activeActionDest = ((ActionZone)interactivePoint).GetJump();
                        break;
                    case UnitActionId.LEAP:
                        unitCtrlr.activeActionDest = ((ActionZone)interactivePoint).GetLeap();
                        break;
                }
            }
        }
        actions.Sort(ActionStatusComparer);
        if (actions.Count > 0)
        {
            if (dir == 0)
            {
                actionIndex = 0;
            }
            else
            {
                actionIndex += dir;
                actionIndex = ((actionIndex < actions.Count) ? ((actionIndex >= 0) ? actionIndex : (actions.Count - 1)) : 0);
            }
            unitCtrlr.SetCurrentAction(actions[actionIndex].SkillId);
            if (unitCtrlr.IsPlayed())
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, actions[actionIndex], actions);
            }
        }
        else if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, null, null);
        }
    }

    private void ShowPointingArrows()
    {
        List<UnitController> list;
        if (unitCtrlr.HasRange())
        {
            ActionStatus action = unitCtrlr.GetAction(SkillId.BASE_SHOOT);
            list = action.Targets;
        }
        else
        {
            list = unitCtrlr.chargeTargets;
        }
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.pointingArrows.Count; i++)
        {
            if (i >= list.Count || list[i] == null)
            {
                PandoraSingleton<MissionManager>.Instance.pointingArrows[i].SetActive(value: false);
                continue;
            }
            PandoraSingleton<MissionManager>.Instance.pointingArrows[i].SetActive(value: true);
            Vector3 toDirection = list[i].transform.position - unitCtrlr.transform.position;
            toDirection.y = 0f;
            PandoraSingleton<MissionManager>.Instance.pointingArrows[i].transform.rotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
        }
    }

    private void HidePointingArrows()
    {
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.pointingArrows.Count; i++)
        {
            PandoraSingleton<MissionManager>.Instance.pointingArrows[i].SetActive(value: false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (unitCtrlr.AICtrlr == null)
        {
            UpdateCurrentAction();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (unitCtrlr.AICtrlr == null)
        {
            UpdateCurrentAction();
        }
    }

    private void UpdateInteractivePoints()
    {
        for (int num = unitCtrlr.interactivePoints.Count - 1; num >= 0; num--)
        {
            if (!unitCtrlr.interactivePoints[num])
            {
                unitCtrlr.interactivePoints.RemoveAt(num);
            }
        }
    }
}
