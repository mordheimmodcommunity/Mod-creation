using UnityEngine;
using UnityEngine.UI;

public class CampaignLoadingView : LoadingView
{
    public Text warbandName;

    public Text rankText;

    public Text ratingText;

    public Image flag;

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

    public override void Show()
    {
        base.Show();
        WarbandSave currentSave = PandoraSingleton<GameManager>.Instance.currentSave;
        string str = ((WarbandId)currentSave.id).ToString();
        warbandName.set_text(currentSave.Name);
        flag.set_sprite(Warband.GetIcon((WarbandId)currentSave.id));
        rankText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_rank_colon_value", currentSave.rank.ToString()));
        LoadBackground("bg_warband_load_" + str);
        Date date = new Date(Constant.GetInt(ConstantId.CAL_DAY_START));
        warbandCreateDate.set_text(date.ToLocalizedAbbrString());
        warbandDaysActive.set_text((currentSave.currentDate - Constant.GetInt(ConstantId.CAL_DAY_START)).ToString());
        int b = currentSave.stats.stats[39];
        missionsAttempted.set_text(b.ToString());
        int num = currentSave.stats.stats[24];
        missionSuccessRate.set_text(((float)num / (float)Mathf.Max(1, b)).ToString("00%"));
        missionCrushingVictories.set_text(currentSave.stats.stats[45].ToString());
        missionTotalVictories.set_text(currentSave.stats.stats[44].ToString());
        b = currentSave.stats.stats[40];
        skirmishesAttempted.set_text(b.ToString());
        num = b - currentSave.stats.stats[28];
        skirmishSuccessRate.set_text(((float)num / (float)Mathf.Max(1, b)).ToString("00%"));
        skirmishDecisiveVictories.set_text(currentSave.stats.stats[43].ToString());
        skirmishObjectiveVictories.set_text(currentSave.stats.stats[26].ToString());
        skirmishBattlegroundVictories.set_text(currentSave.stats.stats[27].ToString());
        int b2 = currentSave.stats.stats[32];
        ooaAllies.set_text(b2.ToString());
        int num2 = currentSave.stats.stats[31];
        ooaEnemies.set_text(num2.ToString());
        outOfActionRatio.set_text(((float)num2 / (float)Mathf.Max(1, b2)).ToString("00%"));
        damageDealt.set_text(currentSave.stats.stats[42].ToString());
        allTimeGold.set_text(currentSave.stats.stats[59].ToString());
        allTimeWyrdFragments.set_text(currentSave.stats.stats[35].ToString());
        allTimeWyrdShards.set_text(currentSave.stats.stats[36].ToString());
        allTimeWyrdClusters.set_text(currentSave.stats.stats[37].ToString());
    }
}
