public class SwitchWeapon : ICheapState
{
	private UnitController unitCtrlr;

	public SwitchWeapon(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.ValidMove();
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		if (unitCtrlr.AICtrlr != null)
		{
			unitCtrlr.AICtrlr.switchCount++;
		}
		PandoraSingleton<MissionManager>.Instance.PlaySequence("switch_weapons", unitCtrlr, OnSeqDone);
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
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UNIT_WEAPON_CHANGED, unitCtrlr);
		unitCtrlr.StateMachine.ChangeState(10);
	}
}
