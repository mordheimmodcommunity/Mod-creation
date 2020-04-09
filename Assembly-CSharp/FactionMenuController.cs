using UnityEngine;

public class FactionMenuController
{
	public Faction Faction
	{
		get;
		private set;
	}

	public WarbandSave WarSave
	{
		get;
		private set;
	}

	public bool HasShipment => Shipment != null;

	public Tuple<int, EventLogger.LogEvent, int> Shipment
	{
		get;
		private set;
	}

	public int ShipmentDate
	{
		get;
		private set;
	}

	public int ShipmentWeight
	{
		get;
		private set;
	}

	public int ShipmentGoldReward
	{
		get;
		set;
	}

	public int NextRankReputation
	{
		get;
		private set;
	}

	public int NextRankReputationModifier
	{
		get;
		private set;
	}

	public int MaxReputationGain
	{
		get;
		private set;
	}

	public int FragmentPrice
	{
		get;
		set;
	}

	public int ShardPrice
	{
		get;
		set;
	}

	public int ClusterPrice
	{
		get;
		set;
	}

	public int PriceModifier
	{
		get;
		set;
	}

	public FactionMenuController(Faction faction, WarbandSave warSave)
	{
		Faction = faction;
		WarSave = warSave;
		FragmentPrice = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(130).PriceSold;
		ShardPrice = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(208).PriceSold;
		ClusterPrice = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(209).PriceSold;
	}

	public void Refresh()
	{
		RefreshBonus();
		RefreshReputation();
		RefreshShipment();
	}

	public void RefreshReputation()
	{
		if (Faction.Rank + 1 >= Faction.RanksData.Count)
		{
			NextRankReputation = -1;
			MaxReputationGain = 0;
			return;
		}
		int num = Faction.Save.rank + 1;
		NextRankReputation = Faction.RanksData[num].Reputation;
		if (NextRankReputationModifier != 0)
		{
			NextRankReputation += NextRankReputation / NextRankReputationModifier;
		}
		MaxReputationGain = NextRankReputation - Faction.Save.reputation;
		if (num + 1 < Faction.RanksData.Count)
		{
			int num2 = 0;
			for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Factions.Count; i++)
			{
				Faction faction = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Factions[i];
				num2 = ((faction != Faction) ? (num2 + faction.Data.RepBonusPercPerOtherFactionRank * faction.Rank) : (num2 + faction.Data.RepBonusPercPerRank * num));
			}
			int num3 = 0;
			if (num2 != 0)
			{
				num3 = 100 / num2;
			}
			int num4 = Faction.RanksData[num + 1].Reputation;
			if (num3 != 0)
			{
				num4 += num4 / num3;
			}
			MaxReputationGain += num4 - 1;
		}
	}

	public void RefreshBonus()
	{
		int num = 100;
		int num2 = 0;
		for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Factions.Count; i++)
		{
			Faction faction = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Factions[i];
			if (faction == Faction)
			{
				num += faction.Data.WyrdstonePriceBonusPercPerRank * Faction.Rank;
				num2 += faction.Data.RepBonusPercPerRank * Faction.Rank;
			}
			else
			{
				num += faction.Data.WyrdstonePriceBonusPercPerOtherFactionRank * faction.Rank;
				num2 += faction.Data.RepBonusPercPerOtherFactionRank * faction.Rank;
			}
		}
		num = Mathf.Max(Faction.Data.MinWydstonePriceModifier, num);
		NextRankReputationModifier = 0;
		if (num2 != 0)
		{
			NextRankReputationModifier = 100 / num2;
		}
		PriceModifier = 0;
		if (num != 100)
		{
			PriceModifier = 100 / (num - 100);
		}
	}

	public int AddReputation(int rep)
	{
		int num = 0;
		if (NextRankReputation != -1)
		{
			Faction.Save.reputation += rep;
			while (NextRankReputation != -1 && Faction.Save.reputation >= NextRankReputation)
			{
				if (num != 0)
				{
					Faction.Save.reputation = NextRankReputation - 1;
					break;
				}
				Faction.Save.reputation -= NextRankReputation;
				num = ++Faction.Save.rank;
				Refresh();
				PandoraSingleton<Pan>.Instance.Narrate("new_reputation" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 3));
			}
		}
		return num;
	}

	public void RefreshShipment()
	{
		if (Faction.Primary)
		{
			Tuple<int, EventLogger.LogEvent, int> tuple = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_REQUEST);
			Tuple<int, EventLogger.LogEvent, int> tuple2 = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindEventAfter(EventLogger.LogEvent.SHIPMENT_LATE, tuple.Item1);
			if (tuple.Item1 <= PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate && tuple2 != null && PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate <= tuple2.Item1)
			{
				SetShipment(tuple2);
			}
			else
			{
				SetShipment(null);
			}
		}
		else
		{
			SetShipment(null);
		}
	}

	private void SetShipment(Tuple<int, EventLogger.LogEvent, int> latestShipment)
	{
		Shipment = latestShipment;
		if (latestShipment != null)
		{
			ShipmentDate = latestShipment.Item1;
			ShipmentWeight = latestShipment.Item3;
			ShipmentGoldReward = GetShipmentDeliveryPrice(ShipmentWeight, ShipmentWeight);
		}
	}

	public int GetDeliveryReputation(int shipmentWeight)
	{
		return shipmentWeight;
	}

	public int GetDeliveryPrice(int shipmentWeight)
	{
		int num = Mathf.FloorToInt((float)shipmentWeight / Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT));
		if (PriceModifier != 0)
		{
			num += num / PriceModifier;
		}
		return num;
	}

	public int GetShipmentDeliveryReputation(int shipmentWeight, int requiredWeight)
	{
		return Mathf.FloorToInt((float)Mathf.Clamp(shipmentWeight - requiredWeight, 0, int.MaxValue) / Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT));
	}

	public int GetShipmentDeliveryPrice(int shipmentWeight, int requiredWeight)
	{
		int shipmentWeight2 = Mathf.Clamp(shipmentWeight - requiredWeight, 0, int.MaxValue);
		int deliveryPrice = GetDeliveryPrice(shipmentWeight2);
		Warband warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
		return Mathf.FloorToInt((float)requiredWeight / Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT) * warband.GetPercAttribute(WarbandAttributeId.REQUEST_PRICE_PERC) * (1f + warband.GetPercAttribute(WarbandAttributeId.REQUEST_PRICE_GLOBAL_PERC))) + deliveryPrice;
	}

	public void CreateNewShipmentRequest(int lastShipmentDueDate, int lastShipmentDeliveryDate)
	{
		WarbandMenuController warbandCtrlr = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr;
		WyrdstoneShipmentData wyrdstoneShipmentData = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneShipmentData>("fk_warband_rank_id", WarSave.rank.ToString())[0];
		int data = Mathf.FloorToInt((float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(wyrdstoneShipmentData.MinWeight, wyrdstoneShipmentData.MaxWeight + 1) * PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetPercAttribute(WarbandAttributeId.REQUEST_WEIGHT_PERC));
		int num = lastShipmentDueDate + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(wyrdstoneShipmentData.NextMinDays, wyrdstoneShipmentData.NextMaxDays + 1);
		WarSave.nextShipmentExtraDays = 0;
		if (num < lastShipmentDeliveryDate)
		{
			num = lastShipmentDeliveryDate;
		}
		warbandCtrlr.Warband.Logger.AddHistory(num, EventLogger.LogEvent.SHIPMENT_REQUEST, data);
	}

	public string GetConsequenceLabel()
	{
		return "faction_consequence_" + Faction.Data.Id + "_" + WarSave.lateShipmentCount;
	}
}
