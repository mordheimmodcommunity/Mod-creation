using System.Collections;

public class WarbandTabsModule : TabsModule
{
	public override void Init()
	{
		base.Init();
		int num = 0;
		AddTabIcon(HideoutManager.State.CAMP, num++, HideoutCamp.NodeSlot.CAMP, null, "hideout_camp");
		AddTabIcon(HideoutManager.State.SHIPMENT, num++, HideoutCamp.NodeSlot.SHIPMENT, null, "hideout_smuggler", IsSmugglersAvailable);
		AddTabIcon(HideoutManager.State.SHOP, num++, HideoutCamp.NodeSlot.SHOP, null, "hideout_shop", IsShopAvailable);
		AddTabIcon(HideoutManager.State.WARBAND, num++, HideoutCamp.NodeSlot.LEADER, null, "hideout_management");
		AddTabIcon(HideoutManager.State.SKIRMISH, num++, HideoutCamp.NodeSlot.BANNER, "icn_mission_type_bounty", "hideout_skirmish", IsSkirmishAvailable);
		AddTabIcon(HideoutManager.State.PLAYER_PROGRESSION, num++, HideoutCamp.NodeSlot.DRAMATIS, "unit/" + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetDramatis().unit.Id.ToLowerString(), "hideout_progression", IsPlayerProgressionAvailable);
		AddTabIcon(HideoutManager.State.MISSION, num++, HideoutCamp.NodeSlot.WAGON, "icn_mission_type_mission", "hideout_campaign", IsMissionAvailable);
		AddTabIcon(HideoutManager.State.CAMP, num++, HideoutCamp.NodeSlot.NEXT_DAY, "action/end_turn", "hideout_day_skip", IsNextDayAvailable);
	}

	protected override void SetCurrentTabs(int index)
	{
		base.SetCurrentTabs(index);
		if (IsTabAvailable(index) && icons[index].nodeSlot == HideoutCamp.NodeSlot.NEXT_DAY)
		{
			TriggerNextDay();
		}
	}

	private void TriggerNextDay()
	{
		StartCoroutine(DelayNextDay());
	}

	private IEnumerator DelayNextDay()
	{
		yield return null;
		PandoraSingleton<HideoutManager>.Instance.OnNextDay();
		Refresh();
	}

	public void AddTabIcon(HideoutManager.State state, int index, HideoutCamp.NodeSlot nodeSlot, string icon = null, string loc = null, IsAvailable isAvailable = null)
	{
		TabIcon tabIcon = AddTabIcon(state, index, icon, loc, isAvailable);
		tabIcon.nodeSlot = nodeSlot;
	}

	public TabIcon GetTabIcon(HideoutCamp.NodeSlot nodeSlot)
	{
		for (int i = 0; i < icons.Count; i++)
		{
			if (icons[i].nodeSlot == nodeSlot)
			{
				return icons[i];
			}
		}
		return null;
	}

	public bool IsShopAvailable(out string reason)
	{
		reason = string.Empty;
		if (PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) && PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Units.Count == 1 && !PandoraSingleton<HideoutManager>.Instance.IsPostMission())
		{
			reason = "na_hideout_new_game";
			return false;
		}
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
		{
			reason = "na_hideout_late_shipment_count";
			return false;
		}
		return true;
	}

	public bool IsSmugglersAvailable(out string reason)
	{
		reason = string.Empty;
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
		{
			reason = "na_hideout_late_shipment_count";
			return false;
		}
		if (PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START))
		{
			reason = "na_hideout_next_day";
			return false;
		}
		return true;
	}

	public bool IsMissionAvailable(out string reason)
	{
		reason = string.Empty;
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
		{
			reason = "na_hideout_late_shipment_count";
			return false;
		}
		if (PandoraSingleton<HideoutManager>.Instance.IsPostMission())
		{
			reason = "na_hideout_post_mission";
			return false;
		}
		if (!PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.HasLeader(needToBeActive: true))
		{
			reason = "na_hideout_active_leader";
			return false;
		}
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetActiveUnitsCount() < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
		{
			reason = "na_hideout_min_active_unit";
			return false;
		}
		return true;
	}

	public bool IsSkirmishAvailable(out string reason)
	{
		reason = string.Empty;
		return PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.IsSkirmishAvailable(out reason);
	}

	public bool IsNextDayAvailable(out string reason)
	{
		if (PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) && !PandoraSingleton<HideoutManager>.Instance.IsPostMission())
		{
			reason = "na_hideout_next_day";
			return false;
		}
		if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
		{
			reason = "na_hideout_late_shipment_count";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	public bool IsPlayerProgressionAvailable(out string reason)
	{
		reason = string.Empty;
		if (PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) && PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Units.Count == 1 && !PandoraSingleton<HideoutManager>.Instance.IsPostMission() && PandoraSingleton<GameManager>.Instance.Profile.CurrentXp == 0)
		{
			reason = "na_hideout_new_game";
			return false;
		}
		return true;
	}
}
