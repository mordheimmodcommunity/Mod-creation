internal class UIMissionTarget : UIMissionMoving
{
	public UIMissionTarget(UIMissionManager uiMissionManager)
		: base(uiMissionManager)
	{
	}

	public override void Enter(int iFrom)
	{
		base.Enter(iFrom);
		if ((base.UiMissionManager.CurrentUnitController != null && base.UiMissionManager.CurrentUnitController.IsPlayed()) || (base.UiMissionManager.CurrentUnitTargetController != null && base.UiMissionManager.ShowingOverview))
		{
			base.UiMissionManager.targetCombatStats.OnEnable();
			PandoraSingleton<UIMissionManager>.Instance.ShowTargetExtraStats();
			if (!base.UiMissionManager.ShowingOverview && base.UiMissionManager.CurrentUnitController != null && base.UiMissionManager.CurrentUnitController.IsCurrentState(UnitController.State.COUNTER_CHOICE))
			{
				base.UiMissionManager.objectives.OnDisable();
				base.UiMissionManager.leftSequenceMessage.Message("reaction_melee_attack");
			}
		}
		else
		{
			base.UiMissionManager.unitAction.WaitingOpponent();
			base.UiMissionManager.targetCombatStats.OnDisable();
			base.UiMissionManager.targetAlternateWeapon.OnDisable();
			base.UiMissionManager.targetStats.OnDisable();
			base.UiMissionManager.targetEnchantments.OnDisable();
		}
	}

	public override void Exit(int iTo)
	{
		if (iTo != 1)
		{
			base.Exit(iTo);
			base.UiMissionManager.targetCombatStats.OnDisable();
			base.UiMissionManager.targetAlternateWeapon.OnDisable();
			base.UiMissionManager.targetStats.OnDisable();
			base.UiMissionManager.targetEnchantments.OnDisable();
			base.UiMissionManager.targetEnchantmentsDebuffs.OnDisable();
		}
	}
}
