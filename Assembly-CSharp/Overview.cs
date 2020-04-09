public class Overview : ICheapState
{
	private UnitController unitCtrlr;

	public Overview(UnitController unit)
	{
		unitCtrlr = unit;
	}

	void ICheapState.Destroy()
	{
		unitCtrlr = null;
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraDebug.LogInfo("Overview Enter", "SUBFLOW");
		PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.OVERVIEW, unitCtrlr.transform);
		PandoraSingleton<MissionManager>.Instance.HideCombatCircles();
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview", -1) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel", -1) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel", -1))
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, unitCtrlr);
			unitCtrlr.StateMachine.ChangeState((!unitCtrlr.Engaged) ? 11 : 12);
		}
	}

	void ICheapState.FixedUpdate()
	{
	}
}
