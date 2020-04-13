using UnityEngine;
using UnityEngine.UI;

public class UIMissionMoraleController : CanvasGroupDisabler
{
    public Color enemyColor;

    public Color playerColor;

    public Slider playerMorale;

    public Text playerMoraleText;

    public Text playerMoraleThresholdText;

    public GameObject enemyMoraleBar;

    public Slider enemyMorale;

    public Text enemyMoraleText;

    public Text enemyMoraleThresholdText;

    public Image compass;

    private WarbandController playerWarbandController;

    private WarbandController enemyWarbandController;

    public Text mapNameText;

    public Text EnemyTurnText;

    public Text timer;

    private float oldTimer = -1f;

    public RectTransform playerRout;

    public RectTransform enemyRout;

    public float sliderHeight;

    private TurnTimer turnTimer;

    private float prevMorale = 1f;

    private void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.WARBAND_MORALE_CHANGED, OnMoraleChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_ROUND_START, OnStart);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_DEPLOY, OnStart);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_CHANGED, OnUnitChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TIMER_STARTING, OnTimerStarting);
    }

    private void OnUnitChanged()
    {
        if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == null)
        {
            EnemyTurnText.set_text(string.Empty);
        }
        else if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().IsPlayed())
        {
            EnemyTurnText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_player_turn"));
            ((Graphic)EnemyTurnText).set_color(playerColor);
        }
        else
        {
            EnemyTurnText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_enemy_turn"));
            ((Graphic)EnemyTurnText).set_color(enemyColor);
        }
    }

    private void OnMoraleChanged()
    {
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
        {
            WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i];
            if (playerWarbandController == warbandController)
            {
                playerMorale.set_normalizedValue(playerWarbandController.MoralRatio);
                playerMoraleText.set_text(playerWarbandController.MoralValue.ToConstantString());
                if (prevMorale >= PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold && playerWarbandController.MoralRatio < PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold)
                {
                    PandoraSingleton<Pan>.Instance.Narrate("morale_low2");
                }
                else if (prevMorale >= 50f && playerWarbandController.MoralRatio < 50f)
                {
                    PandoraSingleton<Pan>.Instance.Narrate("morale_low1");
                }
                prevMorale = playerWarbandController.MoralRatio;
            }
            else if ((!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto) && enemyWarbandController == warbandController)
            {
                enemyMorale.set_normalizedValue(enemyWarbandController.MoralRatio);
                enemyMoraleText.set_text(enemyWarbandController.MoralValue.ToConstantString());
            }
        }
    }

    private void OnStart()
    {
        float routThreshold = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold;
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.MISSION_ROUND_START, OnStart);
        playerWarbandController = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
        playerMorale.set_maxValue((float)playerWarbandController.MaxMoralValue);
        playerMorale.set_normalizedValue(playerWarbandController.MoralRatio);
        playerMoraleText.set_text(playerWarbandController.MoralValue.ToConstantString());
        playerMoraleThresholdText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_moral_threshold", Mathf.FloorToInt(routThreshold * (float)playerWarbandController.MaxMoralValue).ToConstantString()));
        playerRout.anchoredPosition = new Vector2(0f, routThreshold * sliderHeight);
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign && !PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            enemyMoraleBar.SetActive(value: false);
        }
        else
        {
            enemyWarbandController = PandoraSingleton<MissionManager>.Instance.GetMainEnemyWarbandCtrlr();
            enemyMorale.set_maxValue((float)enemyWarbandController.MaxMoralValue);
            enemyMorale.set_normalizedValue(enemyWarbandController.MoralRatio);
            enemyMoraleText.set_text(enemyWarbandController.MoralValue.ToConstantString());
            enemyMoraleThresholdText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_moral_threshold", Mathf.FloorToInt(routThreshold * (float)enemyWarbandController.MaxMoralValue).ToConstantString()));
            enemyRout.anchoredPosition = new Vector2(0f, routThreshold * sliderHeight);
        }
        if (PandoraSingleton<MissionManager>.Instance.mapData != null)
        {
            mapNameText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(PandoraSingleton<MissionManager>.Instance.mapData.Name + "_name"));
            if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign)
            {
                DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployScenarioMapLayoutId);
                Text obj = mapNameText;
                obj.set_text(obj.get_text() + "\n" + PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + deploymentScenarioMapLayoutData.DeploymentScenarioId.ToLowerString()));
            }
        }
        else
        {
            mapNameText.set_text(string.Empty);
        }
    }

    private void Update()
    {
        if (turnTimer != null)
        {
            ((Behaviour)(object)timer).enabled = !turnTimer.Paused;
            if (!turnTimer.Paused && oldTimer != turnTimer.Timer)
            {
                oldTimer = turnTimer.Timer;
                timer.set_text(turnTimer.Timer.ToString("###"));
            }
        }
        Vector3 eulerAngles = PandoraSingleton<MissionManager>.Instance.CamManager.transform.rotation.eulerAngles;
        float y = eulerAngles.y;
        ((Component)(object)compass).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, y));
    }

    private void OnTimerStarting()
    {
        turnTimer = (TurnTimer)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
    }
}
