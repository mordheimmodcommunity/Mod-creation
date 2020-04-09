public class TurnMessage : ICheapState
{
	private UnitController unitCtrlr;

	private bool displayingMessage;

	public TurnMessage(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		displayingMessage = PandoraSingleton<MissionManager>.Instance.MsgManager.DisplayNewTurn();
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_TUTO_MESSAGE, v1: false, string.Empty, v3: false);
		if (PandoraSingleton<MissionManager>.Instance.CurrentLadderIdx >= 0)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_PLAYER_CHANGE);
		}
	}

	void ICheapState.Update()
	{
		if (!displayingMessage)
		{
			unitCtrlr.nextState = UnitController.State.UPDATE_EFFECTS;
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action", 1))
		{
			displayingMessage = PandoraSingleton<MissionManager>.Instance.MsgManager.DisplayNextPos();
		}
	}

	void ICheapState.FixedUpdate()
	{
	}
}
