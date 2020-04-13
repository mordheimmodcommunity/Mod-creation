using UnityEngine.UI;

public class PlayerRankDescModule : UIModule
{
    public Text nextRankBonus;

    public Text currentPerks;

    public void Refresh()
    {
        currentPerks.set_text(PandoraSingleton<GameManager>.Instance.Profile.GetCurrentRankDescription());
        if (PandoraSingleton<GameManager>.Instance.Profile.RankData.XpNeeded > 0)
        {
            nextRankBonus.set_text(PandoraSingleton<GameManager>.Instance.Profile.GetNextRankDescription());
        }
        else
        {
            nextRankBonus.set_text(string.Empty);
        }
    }
}
