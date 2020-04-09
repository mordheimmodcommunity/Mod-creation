public class Disengage : ICheapState
{
	private UnitController unitCtrlr;

	public Disengage(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: false);
		unitCtrlr.SetFleeTarget();
		unitCtrlr.FaceTarget(unitCtrlr.FleeTarget, force: true);
		unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
		PandoraSingleton<MissionManager>.Instance.PlaySequence("disengage", unitCtrlr, OnSeqDone);
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

	public void OnSeqDone()
	{
		unitCtrlr.Ground();
		unitCtrlr.CheckEngaged(applyEnchants: true);
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			unitCtrlr.SendStartMove(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
		}
	}
}
