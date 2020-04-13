using UnityEngine.UI;

public class TreasuryModule : UIModule
{
    public Text holiday;

    public Text date;

    public Text fragments;

    public Text shards;

    public Text clusters;

    public Text totalWeight;

    public Text gold;

    public Text shipment;

    public void Refresh(WarbandSave save)
    {
        Tuple<int, EventLogger.LogEvent, int> tuple = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
        date.set_text(PandoraSingleton<HideoutManager>.Instance.Date.ToLocalizedString());
        holiday.set_text(PandoraSingleton<HideoutManager>.Instance.Date.ToLocalizedHoliday());
        int amount = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_FRAGMENT).amount;
        int amount2 = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_SHARD).amount;
        int amount3 = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_CLUSTER).amount;
        fragments.set_text(amount.ToString());
        shards.set_text(amount2.ToString());
        clusters.set_text(amount3.ToString());
        float @float = Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT);
        amount *= PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(130).PriceSold;
        amount2 *= PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(208).PriceSold;
        amount3 *= PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(209).PriceSold;
        totalWeight.set_text(((float)(amount + amount2 + amount3) * @float).ToString());
        gold.set_text(PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.GOLD).amount.ToString());
        if (tuple != null && tuple.Item1 > PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            shipment.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_days_left", (tuple.Item1 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate).ToString()));
        }
        else if (tuple != null && tuple.Item1 == PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            shipment.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_days_left"));
        }
        else
        {
            shipment.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_shipment"));
        }
    }
}
