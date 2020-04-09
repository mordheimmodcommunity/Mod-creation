internal class UIMissionSequence : ICheapState
{
	public UIMissionManager UiMissionManager
	{
		get;
		set;
	}

	public UIMissionSequence(UIMissionManager missionManager)
	{
		UiMissionManager = missionManager;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		if (PandoraSingleton<MissionManager>.Instance.focusedUnit.IsPlayed() || PandoraSingleton<MissionManager>.Instance.focusedUnit.Imprint.State == MapImprintStateId.VISIBLE)
		{
			UiMissionManager.ladder.OnDisable();
			UiMissionManager.morale.OnDisable();
			UiMissionManager.leftSequenceMessage.OnDisable();
			UiMissionManager.rightSequenceMessage.OnDisable();
			UiMissionManager.objectives.OnDisable();
			foreach (UISlideInElement extraStat in PandoraSingleton<UIMissionManager>.Instance.extraStats)
			{
				extraStat.OnDisable();
			}
		}
		else
		{
			UiMissionManager.unitAction.WaitingOpponent();
		}
	}

	public void Exit(int iTo)
	{
		UiMissionManager.ladder.OnEnable();
		UiMissionManager.morale.OnEnable();
		UiMissionManager.leftSequenceMessage.OnDisable();
		UiMissionManager.rightSequenceMessage.OnDisable();
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}
}
