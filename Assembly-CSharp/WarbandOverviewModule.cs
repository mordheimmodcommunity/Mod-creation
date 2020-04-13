using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarbandOverviewModule : UIModule
{
    [Header("Panels")]
    public ToggleEffects[] tabs;

    public GameObject[] panels;

    [Header("Description")]
    public Text descriptionText;

    [Header("History")]
    public GameObject historyTitleTemplate;

    public GameObject historyEntryTemplate;

    public ScrollGroup historyScrollgroup;

    [Header("Stats")]
    public Text warbandCreateDate;

    public Text warbandDaysActive;

    public Text missionsAttempted;

    public Text missionSuccessRate;

    public Text missionCrushingVictories;

    public Text missionTotalVictories;

    public Text skirmishesAttempted;

    public Text skirmishSuccessRate;

    public Text skirmishDecisiveVictories;

    public Text skirmishObjectiveVictories;

    public Text skirmishBattlegroundVictories;

    public Text ooaAllies;

    public Text ooaEnemies;

    public Text outOfActionRatio;

    public Text damageDealt;

    public Text allTimeGold;

    public Text allTimeWyrdFragments;

    public Text allTimeWyrdShards;

    public Text allTimeWyrdClusters;

    private int tabIdx = -1;

    private new void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabs[i].onSelect.AddListener(delegate
            {
                ShowPanel(idx);
            });
        }
    }

    public void ShowPanel(int panelIdx)
    {
        if (tabIdx == panelIdx)
        {
            return;
        }
        tabIdx = panelIdx;
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIdx)
            {
                tabs[i].SetOn();
            }
            panels[i].SetActive(i == panelIdx);
        }
    }

    public void Set(Warband wb)
    {
        descriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_desc_" + wb.Id.ToLowerString()));
        SetHistoryPanel(wb);
        SetStatsPanel(wb);
    }

    private void SetHistoryPanel(Warband wb)
    {
        historyScrollgroup.ClearList();
        List<Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave>> list = new List<Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave>>();
        for (int i = 0; i < wb.Logger.history.Count; i++)
        {
            list.Add(new Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave>(wb.Logger.history[i], null));
        }
        for (int j = 0; j < wb.GetWarbandSave().oldUnits.Count; j++)
        {
            for (int k = 0; k < wb.GetWarbandSave().oldUnits[j].history.Count; k++)
            {
                list.Add(new Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave>(wb.GetWarbandSave().oldUnits[j].history[k], wb.GetWarbandSave().oldUnits[j]));
            }
        }
        for (int l = 0; l < wb.Units.Count; l++)
        {
            for (int m = 0; m < wb.Units[l].UnitSave.stats.history.Count; m++)
            {
                list.Add(new Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave>(wb.Units[l].UnitSave.stats.history[m], wb.Units[l].UnitSave.stats));
            }
        }
        list.Sort((Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave> a, Tuple<Tuple<int, EventLogger.LogEvent, int>, UnitStatSave> b) => a.Item1.Item1.CompareTo(b.Item1.Item1));
        MonthId monthId = MonthId.MAX_VALUE;
        for (int num = list.Count - 1; num >= 0; num--)
        {
            if (list[num].Item1.Item1 <= PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
            {
                Date date = new Date(list[num].Item1.Item1);
                if (date.Month != monthId)
                {
                    historyScrollgroup.Setup(historyTitleTemplate, hideBarIfEmpty: true);
                    monthId = date.Month;
                    GameObject gameObject = historyScrollgroup.AddToList(null, null);
                    if (monthId == MonthId.NONE)
                    {
                        gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_holiday_" + date.Holiday.ToLowerString()));
                    }
                    else
                    {
                        gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_month_" + monthId.ToLowerString()));
                    }
                    historyScrollgroup.Setup(historyEntryTemplate, hideBarIfEmpty: true);
                }
                string text = string.Empty;
                switch (list[num].Item1.Item2)
                {
                    case EventLogger.LogEvent.HIRE:
                        {
                            UnitStatSave item2 = list[num].Item2;
                            string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_" + ((UnitId)item2.id).ToLowerString());
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_hire", stringById, item2.Name);
                            break;
                        }
                    case EventLogger.LogEvent.FIRE:
                        {
                            UnitStatSave item6 = list[num].Item2;
                            string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_" + ((UnitId)item6.id).ToLowerString());
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_fire", stringById2, item6.Name);
                            break;
                        }
                    case EventLogger.LogEvent.DEATH:
                        {
                            UnitStatSave item5 = list[num].Item2;
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_death", item5.Name);
                            break;
                        }
                    case EventLogger.LogEvent.RETIREMENT:
                        {
                            UnitStatSave item4 = list[num].Item2;
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_retire", item4.Name);
                            break;
                        }
                    case EventLogger.LogEvent.LEFT:
                        {
                            UnitStatSave item3 = list[num].Item2;
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_left", item3.Name);
                            break;
                        }
                    case EventLogger.LogEvent.WARBAND_CREATED:
                        text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_warband_created");
                        break;
                    case EventLogger.LogEvent.MEMORABLE_CAMPAIGN_VICTORY:
                        text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_campaign_victory_total", list[num].Item1.Item3.ToString());
                        break;
                    case EventLogger.LogEvent.VICTORY_STREAK:
                        text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_victory_streak", list[num].Item1.Item3.ToString());
                        break;
                    case EventLogger.LogEvent.RANK_ACHIEVED:
                        {
                            UnitStatSave item = list[num].Item2;
                            if (item == null)
                            {
                                text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_history_warband_rank", list[num].Item1.Item3.ToString());
                            }
                            break;
                        }
                }
                if (!string.IsNullOrEmpty(text))
                {
                    WarbandHistoryItem component = historyScrollgroup.AddToList(null, null).GetComponent<WarbandHistoryItem>();
                    if (date.Day == 0)
                    {
                        component.date.set_text(string.Empty);
                    }
                    else
                    {
                        component.date.set_text(date.Day.ToString());
                    }
                    component.eventDesc.set_text(text);
                }
            }
        }
    }

    private void SetStatsPanel(Warband wb)
    {
        Date date = new Date(Constant.GetInt(ConstantId.CAL_DAY_START));
        warbandCreateDate.set_text(date.ToLocalizedAbbrString());
        warbandDaysActive.set_text((PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate - Constant.GetInt(ConstantId.CAL_DAY_START)).ToString());
        int attribute = wb.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_ATTEMPTED);
        missionsAttempted.set_text(attribute.ToString());
        int attribute2 = wb.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_WIN);
        missionSuccessRate.set_text(((float)attribute2 / (float)Mathf.Max(1, attribute)).ToString("00%"));
        missionCrushingVictories.set_text(wb.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_CRUSHED_VICTORY).ToString());
        missionTotalVictories.set_text(wb.GetAttribute(WarbandAttributeId.CAMPAIGN_MISSION_TOTAL_VICTORY).ToString());
        attribute = wb.GetAttribute(WarbandAttributeId.SKIRMISH_ATTEMPTED);
        skirmishesAttempted.set_text(attribute.ToString());
        attribute2 = attribute - wb.GetAttribute(WarbandAttributeId.SKIRMISH_LOST);
        skirmishSuccessRate.set_text(((float)attribute2 / (float)Mathf.Max(1, attribute)).ToString("00%"));
        skirmishDecisiveVictories.set_text(wb.GetAttribute(WarbandAttributeId.SKIRMISH_DECISIVE_VICTORY).ToString());
        skirmishObjectiveVictories.set_text(wb.GetAttribute(WarbandAttributeId.SKIRMISH_OBJECTIVE_VICTORY).ToString());
        skirmishBattlegroundVictories.set_text(wb.GetAttribute(WarbandAttributeId.SKIRMISH_BATTLEGROUND_VICTORY).ToString());
        int attribute3 = wb.GetAttribute(WarbandAttributeId.TOTAL_OOA);
        ooaAllies.set_text(attribute3.ToString());
        int attribute4 = wb.GetAttribute(WarbandAttributeId.TOTAL_KILL);
        ooaEnemies.set_text(attribute4.ToString());
        outOfActionRatio.set_text(((float)attribute4 / (float)Mathf.Max(1, attribute3)).ToString("00%"));
        damageDealt.set_text(wb.GetAttribute(WarbandAttributeId.TOTAL_DAMAGE).ToString());
        allTimeGold.set_text(wb.GetAttribute(WarbandAttributeId.TOTAL_GOLD).ToString());
        allTimeWyrdFragments.set_text(wb.GetAttribute(WarbandAttributeId.FRAGMENTS_GATHERED).ToString());
        allTimeWyrdShards.set_text(wb.GetAttribute(WarbandAttributeId.SHARDS_GATHERED).ToString());
        allTimeWyrdClusters.set_text(wb.GetAttribute(WarbandAttributeId.CLUSTERS_GATHERED).ToString());
    }

    public void SetNav(Selectable left)
    {
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0012: Unknown result type (might be due to invalid IL or missing references)
        //IL_0028: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)tabs[0].toggle).get_navigation();
        ((Navigation)(ref navigation)).set_selectOnLeft(left);
        ((Selectable)tabs[0].toggle).set_navigation(navigation);
    }

    public void Update()
    {
        if (tabIdx == 1)
        {
            float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("v");
            if (!Mathf.Approximately(axis, 0f))
            {
                historyScrollgroup.ForceScroll(axis < 0f, setSelected: false);
            }
        }
    }
}
