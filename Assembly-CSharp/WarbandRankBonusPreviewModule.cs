using UnityEngine;
using UnityEngine.UI;

public class WarbandRankBonusPreviewModule : UIModule
{
    [SerializeField]
    private Text nextRankBonusField;

    [SerializeField]
    private Text currentRankBonusField;

    public void Set(Warband wb)
    {
        nextRankBonusField.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warband_adv_next_rank_" + (wb.Rank + 1).ToString()));
        currentRankBonusField.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warband_adv_current_rank_" + wb.Rank.ToString()));
    }
}
