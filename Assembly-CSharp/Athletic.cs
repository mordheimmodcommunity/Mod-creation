using UnityEngine;

public class Athletic : ICheapState
{
	private UnitController unitCtrlr;

	private Vector3 targetPos;

	private PrepareAthletic prepareState;

	public Athletic(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		prepareState = (PrepareAthletic)unitCtrlr.StateMachine.GetState(46);
		unitCtrlr.SetFixed(fix: false);
		unitCtrlr.SetKinemantic(kine: true);
		targetPos = unitCtrlr.activeActionDest.destination.transform.position + unitCtrlr.activeActionDest.destination.transform.forward * (0f - unitCtrlr.CapsuleRadius / 2f);
		if (!prepareState.success)
		{
			int damage = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(unitCtrlr.CurrentAction.GetMinDamage(), unitCtrlr.CurrentAction.GetMaxDamage());
			unitCtrlr.ComputeDirectWound(damage, unitCtrlr.CurrentAction.skillData.BypassArmor, unitCtrlr);
		}
		else
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, prepareState.success, PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_success"));
		}
		unitCtrlr.RemoveAthletics();
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		unitCtrlr.currentActionData.SetActionOutcome(prepareState.success);
		string sequence = string.Format("{0}_{1}m{2}{3}", prepareState.actionId.ToLowerString(), prepareState.height, prepareState.success ? string.Empty : "_fail", (prepareState.success || unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION) ? string.Empty : "_ooa");
		bool value = !unitCtrlr.unit.HasMutatedArm();
		unitCtrlr.animator.SetBool(AnimatorIds.sheathe, value);
		PandoraSingleton<MissionManager>.Instance.PlaySequence(sequence, unitCtrlr, SequenceDone);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitCtrlr);
		if (PandoraSingleton<GameManager>.Instance.IsFastForwarded && unitCtrlr.unit.isAI)
		{
			Time.timeScale = 1.5f;
		}
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<GameManager>.Instance.ResetTimeScale();
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void SequenceDone()
	{
		if (unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION)
		{
			unitCtrlr.KillUnit();
			unitCtrlr.SkillRPC(339);
			return;
		}
		unitCtrlr.transform.position = targetPos;
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.CheckEngaged(applyEnchants: true);
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.zoneAoes.Count; i++)
			{
				PandoraSingleton<MissionManager>.Instance.zoneAoes[i].CheckEnterOrExitUnit(unitCtrlr, network: true);
			}
			unitCtrlr.SendAthleticFinished(prepareState.success, prepareState.actionId);
		}
		else
		{
			unitCtrlr.StateMachine.ChangeState(9);
		}
	}
}
