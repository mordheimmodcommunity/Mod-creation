using UnityEngine;

internal class UIMissionEndGame : ICheapState
{
    private float timer;

    public UIMissionManager UiMissionManager
    {
        get;
        set;
    }

    public UIMissionEndGame(UIMissionManager missionManager)
    {
        UiMissionManager = missionManager;
    }

    public void Destroy()
    {
    }

    public void Enter(int iFrom)
    {
        UiMissionManager.ladder.gameObject.SetActive(value: false);
        UiMissionManager.morale.gameObject.SetActive(value: false);
        UiMissionManager.wheel.gameObject.SetActive(value: false);
        UiMissionManager.objectives.gameObject.SetActive(value: false);
        UiMissionManager.unitAction.gameObject.SetActive(value: false);
        UiMissionManager.unitStats.gameObject.SetActive(value: false);
        UiMissionManager.unitCombatStats.gameObject.SetActive(value: false);
        UiMissionManager.unitAlternateWeapon.gameObject.SetActive(value: false);
        UiMissionManager.unitEnchantments.gameObject.SetActive(value: false);
        UiMissionManager.unitEnchantmentsDebuffs.gameObject.SetActive(value: false);
        UiMissionManager.targetAlternateWeapon.gameObject.SetActive(value: false);
        UiMissionManager.targetCombatStats.gameObject.SetActive(value: false);
        UiMissionManager.targetEnchantments.gameObject.SetActive(value: false);
        UiMissionManager.targetStats.gameObject.SetActive(value: false);
        UiMissionManager.leftSequenceMessage.gameObject.SetActive(value: false);
        UiMissionManager.rightSequenceMessage.gameObject.SetActive(value: false);
        UiMissionManager.HideOptions();
        timer = 2.5f;
    }

    public void Exit(int iTo)
    {
    }

    public void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                UiMissionManager.endGameReport.Show();
            }
        }
    }

    public void FixedUpdate()
    {
    }
}
