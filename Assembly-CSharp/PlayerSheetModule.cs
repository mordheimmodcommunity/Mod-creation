using UnityEngine.UI;

public class PlayerSheetModule : UIModule
{
	public Text warbandName;

	public Text rank;

	public Slider xp;

	public Text xpValue;

	public void Refresh()
	{
		warbandName.set_text(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().Name);
		rank.set_text(PandoraSingleton<GameManager>.Instance.Profile.Rank.ToString());
		if (PandoraSingleton<GameManager>.Instance.Profile.RankData.XpNeeded > 0 && PandoraSingleton<GameManager>.Instance.Profile.HasNextRank)
		{
			xp.get_fillRect().gameObject.SetActive((float)PandoraSingleton<GameManager>.Instance.Profile.CurrentXp > 0f);
			xp.set_normalizedValue((float)PandoraSingleton<GameManager>.Instance.Profile.CurrentXp / (float)PandoraSingleton<GameManager>.Instance.Profile.RankData.XpNeeded);
			xpValue.set_text(PandoraSingleton<GameManager>.Instance.Profile.CurrentXp + " / " + PandoraSingleton<GameManager>.Instance.Profile.RankData.XpNeeded);
		}
		else
		{
			xp.get_fillRect().gameObject.SetActive(value: true);
			xp.set_normalizedValue(1f);
			xpValue.set_text(string.Empty);
		}
	}
}
