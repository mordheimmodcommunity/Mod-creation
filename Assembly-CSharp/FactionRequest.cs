using System;
using UnityEngine;
using UnityEngine.UI;

public class FactionRequest : FactionShipment
{
	public Text weightRequirement;

	public Text goldReward;

	public Text daysLeft;

	public Text fragmentWeight;

	public Text shardWeight;

	public Text clusterWeight;

	public Text totalWeight;

	public Text overweightGold;

	public Text overweightReputation;

	public Text consequences;

	public Text notFullShipment;

	public Color normalColor = Color.white;

	public Color cappedColor = Color.red;

	public override void SetFaction(FactionMenuController faction, Action<FactionShipment> send)
	{
		base.SetFaction(faction, send);
		weightRequirement.set_text(base.FactionCtrlr.ShipmentWeight.ToString("D4"));
		goldReward.set_text(base.FactionCtrlr.ShipmentGoldReward.ToString("D4"));
		totalWeight.set_text("0000");
		int num = base.FactionCtrlr.ShipmentDate - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
		daysLeft.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_days_left", num.ToString()));
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lastShipmentFailed)
		{
			((Behaviour)(object)consequences).enabled = true;
			consequences.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("faction_consequence_penalty_" + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount));
		}
		else
		{
			((Behaviour)(object)consequences).enabled = false;
		}
		RefreshTotal();
	}

	protected override void OnSend()
	{
		if (base.TotalWeight >= base.FactionCtrlr.ShipmentWeight)
		{
			base.OnSend();
		}
	}

	protected override void RefreshTotal()
	{
		base.RefreshTotal();
		fragmentWeight.set_text(base.TotalFragmentWeight.ToString("D3"));
		shardWeight.set_text(base.TotalShardWeight.ToString("D3"));
		clusterWeight.set_text(base.TotalClusterWeight.ToString("D3"));
		int num = base.TotalWeight - base.FactionCtrlr.ShipmentWeight;
		sendBtn.SetDisabled(num < 0);
		((Component)(object)notFullShipment).gameObject.SetActive(num < 0);
		num = ((num >= 0) ? num : 0);
		totalWeight.set_text(base.TotalWeight.ToString("D4"));
		int deliveryPrice = base.FactionCtrlr.GetDeliveryPrice(num);
		base.TotalGold = base.FactionCtrlr.ShipmentGoldReward + deliveryPrice;
		overweightGold.set_text(deliveryPrice.ToString("D4"));
		base.TotalReputation = base.TotalWeight;
		if (base.TotalReputation > base.FactionCtrlr.MaxReputationGain)
		{
			base.TotalReputation = base.FactionCtrlr.MaxReputationGain;
			((Graphic)overweightReputation).set_color(cappedColor);
		}
		else
		{
			((Graphic)overweightReputation).set_color(normalColor);
		}
		overweightReputation.set_text(base.TotalReputation.ToString("D4"));
	}
}
