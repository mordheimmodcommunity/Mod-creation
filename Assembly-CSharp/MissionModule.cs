using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionModule : UIModule
{
	public delegate void OnScoutButton();

	public ListGroup calEvents;

	public GameObject item;

	public Text availableScouts;

	public Text scoutCost;

	public ButtonGroup scoutButton;

	private List<ScoutPriceData> priceDatas;

	public void Setup(OnScoutButton scoutCb)
	{
		priceDatas = PandoraSingleton<DataFactory>.Instance.InitData<ScoutPriceData>("warband_rank", PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Rank.ToString());
		scoutButton.SetAction("send_scout", "mission_send_scout");
		scoutButton.OnAction(delegate
		{
			scoutCb();
		}, mouseOnly: false);
		scoutButton.gameObject.SetActive(value: true);
		RefreshScoutButton();
		EventLogger logger = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger;
		int num = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
		List<Tuple<int, EventLogger.LogEvent, int>> eventsFromDay = logger.GetEventsFromDay(num);
		calEvents.Setup(string.Empty, item);
		GameObject gameObject = null;
		for (int i = 0; i < eventsFromDay.Count; i++)
		{
			if (eventsFromDay[i].Item1 > num && eventsFromDay[i].Item2 != EventLogger.LogEvent.NEW_MISSION && eventsFromDay[i].Item2 != EventLogger.LogEvent.SHIPMENT_REQUEST && eventsFromDay[i].Item2 != EventLogger.LogEvent.CONTACT_ITEM)
			{
				num = eventsFromDay[i].Item1;
				gameObject = calEvents.AddToList();
				UIMissionEvent component = gameObject.GetComponent<UIMissionEvent>();
				component.Setup(eventsFromDay, i, num);
			}
		}
	}

	public void RefreshScoutButton()
	{
		WarbandSave warbandSave = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave();
		int num = priceDatas.Count - warbandSave.scoutsSent;
		if (warbandSave.scoutsSent < 0)
		{
			num = 0;
		}
		availableScouts.set_text(num.ToString());
		if (num == 0)
		{
			scoutButton.SetInteractable(inter: false);
			scoutButton.SetDisabled();
			scoutCost.set_text("0");
			availableScouts.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_scouts_available_none"));
			return;
		}
		int num2 = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetScoutCost(priceDatas[warbandSave.scoutsSent]);
		scoutCost.set_text(num2.ToString());
		if (num2 > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
		{
			((Graphic)scoutCost).set_color(Color.red);
			scoutButton.SetDisabled();
		}
		else
		{
			scoutButton.SetDisabled(disabled: false);
		}
	}
}
