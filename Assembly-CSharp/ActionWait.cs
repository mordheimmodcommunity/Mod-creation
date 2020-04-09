public class ActionWait : ICheapState
{
	private UnitController unitCtrlr;

	public ActionWait(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraDebug.LogInfo("ActionWait Enter ", "UNIT_FLOW", unitCtrlr);
		unitCtrlr.SetFixed(fix: true);
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
}
