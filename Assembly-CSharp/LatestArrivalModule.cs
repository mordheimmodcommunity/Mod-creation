using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LatestArrivalModule : UIModule
{
    public Text eventName;

    public Text description;

    public Text emptyListMessage;

    public Text refreshMessage;

    public UIInventoryItemRuneList itemRuneList;

    public void Set(MarketEventId eventId, List<ItemSave> itemsSave, UnityAction<Item> onItemConfirmed, UnityAction<Item> onItemSelected)
    {
        eventName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("market_" + eventId.ToLowerString() + "_name"));
        description.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("market_" + eventId.ToLowerString() + "_desc"));
        Date date = PandoraSingleton<HideoutManager>.Instance.Date;
        int nextDay = date.GetNextDay(WeekDayId.MARKTAG);
        refreshMessage.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("market_refresh", (nextDay - date.CurrentDate).ToString()));
        List<Item> list = new List<Item>();
        for (int i = 0; i < itemsSave.Count; i++)
        {
            list.Add(new Item(itemsSave[i]));
        }
        list.Sort(new CompareItem());
        itemRuneList.SetList(list, onItemConfirmed, onItemSelected, addEmpty: false, displayPrice: false, buy: false, flagSold: true, allowHighlight: false);
    }

    private void Update()
    {
        float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y");
        if (!Mathf.Approximately(axis, 0f))
        {
            itemRuneList.scrollGroup.ForceScroll(axis < 0f, setSelected: false);
        }
    }
}
