internal class UIMissionMoving : ICheapState
{
	protected UIMissionManager UiMissionManager
	{
		get;
		set;
	}

	public UIMissionMoving(UIMissionManager uiMissionManager)
	{
		UiMissionManager = uiMissionManager;
	}

	public void Destroy()
	{
	}

	public virtual void Enter(int iFrom)
	{
		UiMissionManager.interactiveMessage.OnEnable();
		UiMissionManager.morale.OnEnable();
		UiMissionManager.leftSequenceMessage.OnDisable();
		UiMissionManager.rightSequenceMessage.OnDisable();
		if (UiMissionManager.CurrentUnitController != null && (UiMissionManager.CurrentUnitController.IsPlayed() || UiMissionManager.ShowingOverview))
		{
			if (UiMissionManager.ShowingOverview)
			{
				UiMissionManager.unitAction.OnDisable();
			}
			else
			{
				UiMissionManager.unitAction.OnEnable();
			}
			UiMissionManager.unitCombatStats.OnEnable();
			UiMissionManager.ShowUnitExtraStats();
		}
		else
		{
			if (UiMissionManager.ShowingOverview)
			{
				UiMissionManager.unitAction.OnDisable();
			}
			else
			{
				UiMissionManager.unitAction.WaitingOpponent();
			}
			UiMissionManager.unitCombatStats.OnDisable();
			UiMissionManager.unitAlternateWeapon.OnDisable();
			UiMissionManager.unitStats.OnDisable();
			UiMissionManager.unitEnchantments.OnDisable();
			UiMissionManager.unitEnchantmentsDebuffs.OnDisable();
		}
		UiMissionManager.ShowObjectives();
	}

	public virtual void Exit(int iTo)
	{
		if (iTo != 0 && iTo != 1)
		{
			UiMissionManager.interactiveMessage.OnDisable();
			UiMissionManager.unitAction.OnDisable();
			UiMissionManager.unitCombatStats.OnDisable();
			UiMissionManager.unitAlternateWeapon.OnDisable();
			UiMissionManager.unitStats.OnDisable();
			UiMissionManager.unitEnchantments.OnDisable();
			UiMissionManager.unitEnchantmentsDebuffs.OnDisable();
		}
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}
}
