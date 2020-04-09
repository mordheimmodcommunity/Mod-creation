using UnityEngine;
using UnityEngine.UI;

public class WarbandSheetModule : UIModule
{
	private const string UNITS = "{0} / {1}";

	public Text rank;

	public Text warbandName;

	public Text warbandType;

	public Text rating;

	public Text upkeep;

	public Slider xpBar;

	public Text xpValue;

	public Text units;

	public Text reserve;

	public Text unavailable;

	public Image warbandIcon;

	public void Set(Warband warband)
	{
		rank.set_text(warband.Rank.ToString());
		warbandType.set_text(Warband.GetLocalizedName(warband.Id));
		warbandName.set_text(warband.GetWarbandSave().Name);
		rating.set_text(warband.GetRating().ToString());
		upkeep.set_text(warband.GetTotalUpkeepOwned().ToString());
		if (warband.Rank < warband.GetMaxRank())
		{
			WarbandRankData nextWarbandRankData = warband.GetNextWarbandRankData();
			xpBar.get_fillRect().gameObject.SetActive(warband.Xp > 0);
			xpBar.set_value((float)warband.Xp / (float)nextWarbandRankData.Exp);
			((Behaviour)(object)xpValue).enabled = true;
			xpValue.set_text(warband.Xp + " / " + nextWarbandRankData.Exp);
		}
		else
		{
			xpBar.get_fillRect().gameObject.SetActive(value: true);
			xpBar.set_value(1f);
			((Behaviour)(object)xpValue).enabled = false;
		}
		units.set_text($"{warband.GetNbActiveUnits()} / {warband.GetNbMaxActiveSlots()}");
		reserve.set_text($"{warband.GetNbReserveUnits()} / {warband.GetNbMaxReserveSlot()}");
		unavailable.set_text(warband.GetNbInactiveUnits().ToString());
		warbandIcon.set_sprite(Warband.GetIcon(warband.Id));
	}
}
