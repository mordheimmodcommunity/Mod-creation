using System;
using UnityEngine;
using UnityEngine.UI;

public class FactionDelivery : FactionShipment
{
	public Text totalRep;

	public Text totalGold;

	public Text fragmentRep;

	public Text shardRep;

	public Text clusterRep;

	public Text fragmentPrice;

	public Text shardPrice;

	public Text clusterPrice;

	public Color normalColor = Color.white;

	public Color cappedColor = Color.red;

	public override void SetFaction(FactionMenuController faction, Action<FactionShipment> send)
	{
		base.SetFaction(faction, send);
		fragmentRep.set_text("000");
		shardRep.set_text("000");
		clusterRep.set_text("000");
		fragmentPrice.set_text("000");
		shardPrice.set_text("000");
		clusterPrice.set_text("000");
		totalRep.set_text("0000");
		totalGold.set_text("0000");
		((Graphic)fragmentRep).set_color(normalColor);
		((Graphic)shardRep).set_color(normalColor);
		((Graphic)clusterRep).set_color(normalColor);
		((Graphic)totalRep).set_color(normalColor);
	}

	protected override void RefreshTotal()
	{
		base.RefreshTotal();
		int num = base.TotalFragmentWeight;
		int num2 = base.TotalShardWeight;
		int num3 = base.TotalClusterWeight;
		int num4 = base.FactionCtrlr.FragmentPrice;
		int num5 = base.FactionCtrlr.ShardPrice;
		int num6 = base.FactionCtrlr.ClusterPrice;
		if (base.FactionCtrlr.PriceModifier != 0)
		{
			num4 += base.FactionCtrlr.FragmentPrice / base.FactionCtrlr.PriceModifier;
			num5 += base.FactionCtrlr.ShardPrice / base.FactionCtrlr.PriceModifier;
			num6 += base.FactionCtrlr.ClusterPrice / base.FactionCtrlr.PriceModifier;
		}
		int num7 = base.FragmentCount * num4;
		int num8 = base.ShardCount * num5;
		int num9 = base.ClusterCount * num6;
		fragmentPrice.set_text(num7.ToString("D3"));
		shardPrice.set_text(num8.ToString("D3"));
		clusterPrice.set_text(num9.ToString("D3"));
		base.TotalGold = num7 + num8 + num9;
		totalGold.set_text(base.TotalGold.ToString("D4"));
		base.TotalReputation = num + num2 + num3;
		if (base.TotalReputation > base.FactionCtrlr.MaxReputationGain)
		{
			base.TotalReputation = base.FactionCtrlr.MaxReputationGain;
			if (num > base.FactionCtrlr.MaxReputationGain)
			{
				num = base.FactionCtrlr.MaxReputationGain;
			}
			if (num + num2 > base.FactionCtrlr.MaxReputationGain)
			{
				num2 = Mathf.Clamp(num2, 0, base.FactionCtrlr.MaxReputationGain - num);
			}
			if (num + num2 + num3 > base.FactionCtrlr.MaxReputationGain)
			{
				num3 = Mathf.Clamp(num3, 0, base.FactionCtrlr.MaxReputationGain - (num + num2));
			}
			((Graphic)fragmentRep).set_color(cappedColor);
			((Graphic)shardRep).set_color(cappedColor);
			((Graphic)clusterRep).set_color(cappedColor);
			((Graphic)totalRep).set_color(cappedColor);
		}
		else
		{
			((Graphic)fragmentRep).set_color(normalColor);
			((Graphic)shardRep).set_color(normalColor);
			((Graphic)clusterRep).set_color(normalColor);
			((Graphic)totalRep).set_color(normalColor);
		}
		fragmentRep.set_text(num.ToString("D3"));
		shardRep.set_text(num2.ToString("D3"));
		clusterRep.set_text(num3.ToString("D3"));
		totalRep.set_text(base.TotalReputation.ToString("D4"));
	}
}
