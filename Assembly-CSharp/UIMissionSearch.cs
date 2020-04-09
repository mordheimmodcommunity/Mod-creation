internal class UIMissionSearch : ICheapState
{
	public UIMissionManager UiMissionManager
	{
		get;
		set;
	}

	public UIMissionSearch(UIMissionManager missionManager)
	{
		UiMissionManager = missionManager;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		if (PandoraSingleton<MissionManager>.Instance.focusedUnit.IsPlayed())
		{
			UiMissionManager.ladder.OnDisable();
			UiMissionManager.morale.OnDisable();
			UiMissionManager.leftSequenceMessage.OnDisable();
			UiMissionManager.rightSequenceMessage.OnDisable();
			UiMissionManager.objectives.OnDisable();
			UiMissionManager.inventory.Show();
			foreach (UISlideInElement extraStat in PandoraSingleton<UIMissionManager>.Instance.extraStats)
			{
				extraStat.OnDisable();
			}
		}
	}

	public void Exit(int iTo)
	{
		UiMissionManager.inventory.OnDisable();
		UiMissionManager.ladder.OnEnable();
		UiMissionManager.morale.OnEnable();
		UiMissionManager.leftSequenceMessage.OnDisable();
		UiMissionManager.rightSequenceMessage.OnDisable();
		UiMissionManager.ShowObjectives();
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}
}
