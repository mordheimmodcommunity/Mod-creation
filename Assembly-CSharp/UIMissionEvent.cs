using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMissionEvent : MonoBehaviour
{
    public ListGroup itemList;

    public GameObject eventItem;

    public void Setup(List<Tuple<int, EventLogger.LogEvent, int>> log, int index, int day)
    {
        Date date = new Date(day);
        itemList.SetupLocalized(date.ToLocalizedString(), eventItem);
        while (index < log.Count && log[index].Item1 == day)
        {
            switch (log[index].Item2)
            {
                case EventLogger.LogEvent.SHIPMENT_LATE:
                    AddItemToList(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_next_shipment"), log[index].Item3.ToConstantString(), hideImage: false);
                    break;
                case EventLogger.LogEvent.SHIPMENT_DELIVERY:
                case EventLogger.LogEvent.FACTION0_DELIVERY:
                case EventLogger.LogEvent.FACTION1_DELIVERY:
                case EventLogger.LogEvent.FACTION2_DELIVERY:
                    AddItemToList(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_delivery_received_future"));
                    break;
                case EventLogger.LogEvent.MARKET_ROTATION:
                    AddItemToList(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_refresh_market_future"));
                    break;
                case EventLogger.LogEvent.OUTSIDER_ROTATION:
                    AddItemToList(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_refresh_outsider_future"));
                    break;
                case EventLogger.LogEvent.RESPEC_POINT:
                    AddItemToList(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_new_respec_point"));
                    break;
            }
            index++;
        }
    }

    private void AddItemToList(string title, string desc = null, bool hideImage = true)
    {
        GameObject gameObject = itemList.AddToList();
        UIDescription component = gameObject.GetComponent<UIDescription>();
        component.SetLocalized(title, desc);
        if (hideImage)
        {
            ((Component)(object)gameObject.GetComponentsInChildren<Image>()[0]).gameObject.SetActive(value: false);
        }
    }
}
