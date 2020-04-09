using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyReportPopup : ConfirmationPopupView
{
	public Image icon;

	public Text date;

	public Text holiday;

	public ListGroup units;

	public ListGroup warband;

	public GameObject unitItem;

	public GameObject warbandItem;

	public int maxItemsPerPage = 5;

	private List<UnitMenuController> unitList;

	private List<Tuple<Sprite, string>> overflowUnits = new List<Tuple<Sprite, string>>();

	private List<string> overflowWarbands = new List<string>();

	private Action<bool> onDone;

	private int displayedItems;

	public void SetUnitList(List<UnitMenuController> units)
	{
		unitList = units;
	}

	public override void Show(Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		base.Show(OnContinue, hideButtons, hideCancel);
		onDone = callback;
		displayedItems = 0;
		overflowUnits.Clear();
		overflowWarbands.Clear();
		warband.gameObject.SetActive(value: true);
		units.gameObject.SetActive(value: true);
		Sprite sprite = Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id);
		icon.set_sprite(sprite);
		date.set_text(PandoraSingleton<HideoutManager>.Instance.Date.ToLocalizedString());
		holiday.set_text(PandoraSingleton<HideoutManager>.Instance.Date.ToLocalizedHoliday());
		int currentDate = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
		units.Setup(string.Empty, unitItem);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < unitList.Count; i++)
		{
			switch (unitList[i].unit.GetActiveStatus())
			{
			case UnitActiveStatusId.INJURED:
				num++;
				break;
			case UnitActiveStatusId.UPKEEP_NOT_PAID:
				num3++;
				break;
			case UnitActiveStatusId.IN_TRAINING:
				num2++;
				break;
			case UnitActiveStatusId.TREATMENT_NOT_PAID:
				num4++;
				break;
			case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
				num++;
				num3++;
				break;
			}
		}
		bool flag = false;
		for (int j = 0; j < unitList.Count; j++)
		{
			string name = unitList[j].unit.Name;
			Sprite unitIcon = unitList[j].unit.GetIcon();
			EventLogger logger = unitList[j].unit.Logger;
			List<Tuple<int, EventLogger.LogEvent, int>> eventsFromDay = logger.GetEventsFromDay(currentDate - 1);
			for (int k = 0; k < eventsFromDay.Count; k++)
			{
				GameObject gameObject = null;
				UIIconDesc uIIconDesc = null;
				Tuple<int, EventLogger.LogEvent, int> tuple = eventsFromDay[k];
				switch (tuple.Item2)
				{
				case EventLogger.LogEvent.LEFT:
					if (tuple.Item1 == currentDate)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warning_departed", name));
					}
					else if (tuple.Item1 == currentDate + 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_warning_upkeep", name));
					}
					else if (num3 == 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_upkeep_single", name, unitList[j].unit.GetUpkeepOwned().ToString()));
					}
					break;
				case EventLogger.LogEvent.DEATH:
					if (tuple.Item1 == currentDate)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_complication_death", name));
					}
					break;
				case EventLogger.LogEvent.RECOVERY:
					if (tuple.Item1 == currentDate)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_recovered", name, PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + (InjuryId)tuple.Item3)));
					}
					else if (num == 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_in_treatment_single", name, PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + (InjuryId)tuple.Item3), (tuple.Item1 - currentDate).ToString()));
					}
					break;
				case EventLogger.LogEvent.NO_TREATMENT:
					if (tuple.Item1 == currentDate + 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_complication", name));
					}
					else if (num4 == 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_awaiting_treatment_single", name, PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + (InjuryId)tuple.Item3)));
					}
					break;
				case EventLogger.LogEvent.SKILL:
					if (tuple.Item1 == currentDate)
					{
						flag = true;
						string text = null;
						SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>(tuple.Item3);
						text = ((skillData.SkillTypeId != SkillTypeId.SPELL_ACTION) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_learned_skill", name, SkillHelper.GetLocalizedName(skillData)) : PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_learned_spell", name, SkillHelper.GetLocalizedName(skillData)));
						AddUnitMessage(unitIcon, text);
					}
					else if (num2 == 1)
					{
						flag = true;
						AddUnitMessage(unitIcon, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_training_single", name, SkillHelper.GetLocalizedName((SkillId)tuple.Item3), (tuple.Item1 - currentDate).ToString()));
					}
					break;
				}
			}
		}
		if (num3 > 1)
		{
			flag = true;
			int totalUpkeepOwned = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetTotalUpkeepOwned();
			AddUnitMessage(sprite, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_upkeep_multiple", num3.ToString(), totalUpkeepOwned.ToString()));
		}
		if (num4 > 1)
		{
			flag = true;
			AddUnitMessage(sprite, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_awaiting_treatment_multiple", num4.ToString()));
		}
		if (num > 1)
		{
			flag = true;
			AddUnitMessage(sprite, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_in_treatment_multiple", num.ToString()));
		}
		if (num2 > 1)
		{
			flag = true;
			AddUnitMessage(sprite, PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_training_multiple", num2.ToString()));
		}
		if (!flag)
		{
			AddUnitMessage(Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id), PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_injuries_none"));
		}
		warband.Setup(string.Empty, warbandItem);
		EventLogger logger2 = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger;
		List<Tuple<int, EventLogger.LogEvent, int>> eventsAtDay = logger2.GetEventsAtDay(currentDate);
		for (int l = 0; l < eventsAtDay.Count; l++)
		{
			switch (eventsAtDay[l].Item2)
			{
			case EventLogger.LogEvent.NEW_MISSION:
				if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx <= Constant.GetInt(ConstantId.CAMPAIGN_LAST_MISSION))
				{
					string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.PrimaryFactionController.Faction.Data.Desc + "_name");
					AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_mission_request", stringById));
				}
				break;
			case EventLogger.LogEvent.FACTION0_DELIVERY:
			case EventLogger.LogEvent.FACTION1_DELIVERY:
			case EventLogger.LogEvent.FACTION2_DELIVERY:
				ShowDeliveryMessage(eventsAtDay[l]);
				break;
			case EventLogger.LogEvent.MARKET_ROTATION:
				AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_refresh_market"));
				PandoraSingleton<Pan>.Instance.Narrate("market_refresh");
				break;
			case EventLogger.LogEvent.RESPEC_POINT:
				AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_new_respec_point"));
				break;
			case EventLogger.LogEvent.OUTSIDER_ROTATION:
				AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_refresh_outsider"));
				break;
			case EventLogger.LogEvent.CONTACT_ITEM:
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.DecodeContactItemData(eventsAtDay[l].Item3, out WarbandContactId contactId, out ItemId itemId, out int itemQualityId);
				if (itemId == ItemId.GOLD)
				{
					AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_contact_gold", "#warband_skill_title_contact_" + contactId, itemQualityId.ToString()));
				}
				else
				{
					AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_contact", "#warband_skill_title_contact_" + contactId, "#item_quality_" + (ItemQualityId)itemQualityId, Item.GetLocalizedName(itemId, (ItemQualityId)itemQualityId)));
				}
				break;
			}
			}
		}
		ShowShipmentMessage(logger2);
		if (warband.items.Count == 0 && overflowWarbands.Count == 0)
		{
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("daily_report_warband_none"));
		}
		warband.gameObject.SetActive(warband.items.Count > 0);
		units.gameObject.SetActive(units.items.Count > 0);
	}

	private void ShowShipmentMessage(EventLogger warLog)
	{
		int currentDate = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
		Tuple<int, EventLogger.LogEvent, int> tuple = warLog.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
		Tuple<int, EventLogger.LogEvent, int> tuple2 = warLog.FindLastEvent(EventLogger.LogEvent.SHIPMENT_REQUEST);
		Tuple<int, EventLogger.LogEvent, int> tuple3 = warLog.FindLastEvent(EventLogger.LogEvent.SHIPMENT_DELIVERY);
		string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.PrimaryFactionController.Faction.Data.Desc + "_name");
		Tuple<int, EventLogger.LogEvent, int> tuple4 = warLog.FindEventBetween(EventLogger.LogEvent.SHIPMENT_LATE, currentDate, currentDate);
		if (tuple2 != null && tuple2.Item1 == currentDate)
		{
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_shipment_request", stringById));
			PandoraSingleton<Pan>.Instance.Narrate("shipment_request" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 3));
		}
		else if (tuple4 != null && tuple4.Item1 == currentDate)
		{
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_shipment_last_day"));
			PandoraSingleton<Pan>.Instance.Narrate("1_day_left");
		}
		else if (tuple != null && tuple.Item1 + 1 == currentDate)
		{
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_shipment_late"));
		}
		else if (tuple3 != null && tuple3.Item1 == currentDate)
		{
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_shipment_received", stringById));
		}
		else if (tuple != null && tuple2 != null && currentDate > tuple2.Item1 && tuple.Item1 > currentDate)
		{
			int num = tuple.Item1 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
			AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_next_shipment_long", num.ToString()));
			if (num == 3)
			{
				PandoraSingleton<Pan>.Instance.Narrate("3_days_left");
			}
		}
	}

	private void ShowDeliveryMessage(Tuple<int, EventLogger.LogEvent, int> factiondelivery)
	{
		string text = null;
		for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count; i++)
		{
			FactionMenuController factionMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i];
			if (factionMenuController.Faction.GetFactionDeliveryEvent() == factiondelivery.Item2)
			{
				ShipmentSave delivery = factionMenuController.Faction.GetDelivery(factiondelivery.Item3);
				text = PandoraSingleton<LocalizationManager>.Instance.GetStringById(factionMenuController.Faction.Data.Desc + "_name");
				factionMenuController.Faction.ClearDelivery(factiondelivery.Item3);
			}
		}
		AddWarbandMessage(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_delivery_received", text));
	}

	private void AddWarbandMessage(string text)
	{
		if (displayedItems >= maxItemsPerPage)
		{
			overflowWarbands.Add(text);
		}
		else
		{
			GameObject gameObject = warband.AddToList();
			Text val = gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0];
			val.set_text(text);
		}
		displayedItems++;
	}

	private void AddUnitMessage(Sprite unitIcon, string text)
	{
		if (displayedItems >= maxItemsPerPage)
		{
			overflowUnits.Add(new Tuple<Sprite, string>(unitIcon, text));
		}
		else
		{
			GameObject gameObject = units.AddToList();
			UIIconDesc component = gameObject.GetComponent<UIIconDesc>();
			component.SetLocalized(unitIcon, text);
		}
		displayedItems++;
	}

	private void OnContinue(bool confirm)
	{
		if (overflowUnits.Count > 0 || overflowWarbands.Count > 0)
		{
			displayedItems = 0;
			List<Tuple<Sprite, string>> list = new List<Tuple<Sprite, string>>();
			list.AddRange(overflowUnits);
			List<string> list2 = new List<string>();
			list2.AddRange(overflowWarbands);
			overflowUnits.Clear();
			overflowWarbands.Clear();
			for (int i = 0; i < warband.items.Count; i++)
			{
				warband.items[i].gameObject.SetActive(value: false);
			}
			for (int j = 0; j < units.items.Count; j++)
			{
				units.items[j].gameObject.SetActive(value: false);
			}
			for (int k = 0; k < list.Count; k++)
			{
				AddUnitMessage(list[k].Item1, list[k].Item2);
			}
			for (int l = 0; l < list2.Count; l++)
			{
				AddWarbandMessage(list2[l]);
			}
			warband.gameObject.SetActive(list2.Count > 0 && overflowWarbands.Count != list2.Count);
			units.gameObject.SetActive(list.Count > 0 && overflowUnits.Count != list.Count);
			base.Show(OnContinue);
		}
		else
		{
			onDone(confirm);
		}
	}
}
