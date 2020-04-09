using System.Collections;

public class StartMove : ICheapState
{
	private UnitController unitCtrlr;

	private bool actionAvailables;

	private int fixedUpdateCount;

	public StartMove(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		fixedUpdateCount = 0;
		unitCtrlr.startPosition = unitCtrlr.transform.position;
		unitCtrlr.startRotation = unitCtrlr.transform.rotation;
		unitCtrlr.SetKinemantic(kine: false);
		unitCtrlr.attackerCtrlr = null;
		unitCtrlr.defenderCtrlr = null;
		unitCtrlr.friendlyEntered.Clear();
		unitCtrlr.interactivePoint = null;
		unitCtrlr.activeActionDest = null;
		unitCtrlr.prevInteractiveTarget = null;
		unitCtrlr.nextInteractiveTarget = null;
		unitCtrlr.wyrdstoneRollModifier = 0;
		unitCtrlr.attackResultId = AttackResultId.NONE;
		unitCtrlr.lastActionWounds = 0;
		unitCtrlr.actionOutcomeLabel = string.Empty;
		unitCtrlr.flyingLabel = string.Empty;
		if (PandoraSingleton<Hermes>.Instance.IsHost() && PandoraSingleton<MissionManager>.Instance.interruptingUnit == unitCtrlr)
		{
			PandoraSingleton<MissionManager>.Instance.interruptingUnit = null;
		}
		unitCtrlr.unit.UpdateValidNextActionEnchantments();
		if (unitCtrlr.LastActivatedAction != null)
		{
			unitCtrlr.TriggerEnchantments(EnchantmentTriggerId.ON_POST_ACTION, unitCtrlr.LastActivatedAction.SkillId, unitCtrlr.LastActivatedAction.ActionId);
			unitCtrlr.LastActivatedAction = null;
		}
		unitCtrlr.SetCurrentAction(SkillId.NONE);
		unitCtrlr.unit.ResetTempPoints();
		PandoraDebug.LogDebug("StartMove Enter - its turn " + (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit()) + " - status : " + unitCtrlr.unit.Status + " - wasEngaged = " + unitCtrlr.wasEngaged + " IsEngaged Now = " + unitCtrlr.Engaged, "FLOW", unitCtrlr);
		PandoraSingleton<MissionManager>.Instance.ClearBeacons();
		unitCtrlr.currentActionData.Reset();
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION_CLEAR);
		unitCtrlr.StopCoroutine(GotoNextState());
		unitCtrlr.StartCoroutine(GotoNextState());
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
	}

	void ICheapState.FixedUpdate()
	{
		fixedUpdateCount++;
	}

	private IEnumerator GotoNextState()
	{
		while (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating)
		{
			yield return null;
		}
		while (fixedUpdateCount <= 1)
		{
			yield return null;
		}
		unitCtrlr.transform.position = unitCtrlr.startPosition;
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.SetCombatCircle(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit());
		while (PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating)
		{
			yield return null;
		}
		unitCtrlr.wasEngaged = unitCtrlr.Engaged;
		unitCtrlr.CheckEngaged(applyEnchants: true);
		if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
		{
			unitCtrlr.UpdateTargetsData();
			PandoraSingleton<MissionManager>.Instance.TurnTimer.Resume();
			if (unitCtrlr.IsPlayed())
			{
				PandoraSingleton<MissionManager>.Instance.ShowCombatCircles(unitCtrlr);
				PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(unitCtrlr);
			}
			else
			{
				PandoraSingleton<MissionManager>.Instance.HideCombatCircles();
				PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
				PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.WATCH, PandoraSingleton<MissionManager>.Instance.CamManager.Target);
			}
			unitCtrlr.UpdateActionStatus(notice: false);
			actionAvailables = (unitCtrlr.availableActionStatus.Count > 0);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, unitCtrlr);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_START_MOVE);
		}
		if (PandoraSingleton<MissionManager>.Instance.gameFinished || PandoraSingleton<MissionManager>.Instance.CheckEndGame())
		{
			unitCtrlr.StateMachine.ChangeState(9);
		}
		else if (unitCtrlr.IsMine())
		{
			if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
			{
				if (unitCtrlr.unit.IsAvailable() && actionAvailables)
				{
					if (unitCtrlr.AICtrlr == null)
					{
						Engaged engaged = (Engaged)unitCtrlr.StateMachine.GetState(12);
						engaged.actionIndex = 0;
						Moving move = (Moving)unitCtrlr.StateMachine.GetState(11);
						move.actionIndex = 0;
						unitCtrlr.StateMachine.ChangeState(11);
					}
					else
					{
						unitCtrlr.ClampToNavMesh();
						unitCtrlr.StateMachine.ChangeState(42);
						PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_SHOW_ENEMY, v1: false);
					}
				}
				else
				{
					unitCtrlr.SendSkill(SkillId.BASE_END_TURN);
				}
			}
			else
			{
				unitCtrlr.StateMachine.ChangeState(9);
			}
		}
		else if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
		{
			unitCtrlr.StateMachine.ChangeState(43);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_SHOW_ENEMY, v1: false);
		}
		else
		{
			unitCtrlr.StateMachine.ChangeState(9);
		}
		if (unitCtrlr != PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
		{
			unitCtrlr.ActionDone();
		}
	}
}
