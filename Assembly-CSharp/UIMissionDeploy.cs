internal class UIMissionDeploy : ICheapState
{
    protected UIMissionManager UiMissionManager
    {
        get;
        set;
    }

    public UIMissionDeploy(UIMissionManager uiMissionManager)
    {
        UiMissionManager = uiMissionManager;
    }

    public void Destroy()
    {
    }

    public virtual void Enter(int iFrom)
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_CHANGED, ShowUnitUi);
        UiMissionManager.morale.OnEnable();
        UiMissionManager.ladder.OnEnable();
        UiMissionManager.objectives.OnDisable();
        ShowUnitUi();
    }

    private void ShowUnitUi()
    {
        if (UiMissionManager.CurrentUnitController != null && UiMissionManager.CurrentUnitController.IsPlayed())
        {
            UiMissionManager.unitCombatStats.OnEnable();
            UiMissionManager.ShowUnitExtraStats();
            UiMissionManager.deployControls.OnEnable();
            UiMissionManager.unitAction.OnDisable();
        }
        else
        {
            UiMissionManager.unitCombatStats.OnDisable();
            UiMissionManager.unitAlternateWeapon.OnDisable();
            UiMissionManager.unitStats.OnDisable();
            UiMissionManager.unitEnchantments.OnDisable();
            UiMissionManager.unitEnchantmentsDebuffs.OnDisable();
            UiMissionManager.deployControls.OnDisable();
            UiMissionManager.unitAction.WaitingOpponent();
        }
    }

    public virtual void Exit(int iTo)
    {
        UiMissionManager.deployControls.OnDisable();
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.CURRENT_UNIT_CHANGED, ShowUnitUi);
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
    }
}
