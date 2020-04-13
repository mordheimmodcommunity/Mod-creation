using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class FactionShipment : MonoBehaviour
{
    protected Action<FactionShipment> sendCallback;

    public ButtonGroup sendBtn;

    public SelectorGroup fragDel;

    public SelectorGroup shardDel;

    public SelectorGroup clusterDel;

    public ToggleEffects fragGroup;

    public ToggleEffects shardGroup;

    public ToggleEffects clusterGroup;

    public ToggleGroup toggleGroup;

    public Text deliveryCount;

    public Text deliveryDate;

    public FactionMenuController FactionCtrlr
    {
        get;
        set;
    }

    public int TotalFragmentWeight
    {
        get;
        set;
    }

    public int TotalShardWeight
    {
        get;
        set;
    }

    public int TotalClusterWeight
    {
        get;
        set;
    }

    public int FragmentCount => fragDel.CurSel;

    public int ShardCount => shardDel.CurSel;

    public int ClusterCount => clusterDel.CurSel;

    public int TotalGold
    {
        get;
        set;
    }

    public int TotalWeight
    {
        get;
        set;
    }

    public int TotalReputation
    {
        get;
        set;
    }

    public virtual void SetFaction(FactionMenuController faction, Action<FactionShipment> send)
    {
        FactionCtrlr = faction;
        sendCallback = send;
        TotalGold = 0;
        TotalWeight = 0;
        TotalReputation = 0;
        sendBtn.SetAction("send_shipment", "hideout_send");
        sendBtn.OnAction(OnSend, mouseOnly: false);
        sendBtn.SetDisabled();
        WarbandChest warbandChest = PandoraSingleton<HideoutManager>.Instance.WarbandChest;
        int num = Mathf.Min(999, warbandChest.GetItem(ItemId.WYRDSTONE_FRAGMENT).amount);
        fragDel.onValueChanged = OnDeliveryChange;
        fragDel.selections.Clear();
        for (int i = 0; i < num + 1; i++)
        {
            fragDel.selections.Add(i.ToString("D3"));
        }
        fragDel.SetCurrentSel(0);
        num = Mathf.Min(999, warbandChest.GetItem(ItemId.WYRDSTONE_SHARD).amount);
        shardDel.onValueChanged = OnDeliveryChange;
        shardDel.selections.Clear();
        for (int j = 0; j < num + 1; j++)
        {
            shardDel.selections.Add(j.ToString("D3"));
        }
        shardDel.SetCurrentSel(0);
        num = Mathf.Min(999, warbandChest.GetItem(ItemId.WYRDSTONE_CLUSTER).amount);
        clusterDel.onValueChanged = OnDeliveryChange;
        clusterDel.selections.Clear();
        for (int k = 0; k < num + 1; k++)
        {
            clusterDel.selections.Add(k.ToString("D3"));
        }
        clusterDel.SetCurrentSel(0);
        List<Tuple<int, EventLogger.LogEvent, int>> list = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindEventsAfter(faction.Faction.GetFactionDeliveryEvent(), PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + 1);
        int num2 = 0;
        List<Tuple<int, EventLogger.LogEvent, int>> list2 = null;
        if (faction.Faction.Primary)
        {
            list2 = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindEventsAfter(EventLogger.LogEvent.SHIPMENT_DELIVERY, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate + 1);
            num2 = list2.Count;
        }
        deliveryCount.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("delivery_count", (list.Count + num2).ToString()));
        if (list.Count + num2 > 0)
        {
            ((Component)(object)deliveryDate).gameObject.SetActive(value: true);
            int num3 = int.MaxValue;
            for (int l = 0; l < list.Count; l++)
            {
                if (list[l].Item1 < num3)
                {
                    num3 = list[l].Item1;
                }
            }
            for (int m = 0; m < num2; m++)
            {
                if (list2[m].Item1 < num3)
                {
                    num3 = list2[m].Item1;
                }
            }
            int num4 = num3 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
            deliveryDate.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_next_delivery", num4.ToString()));
        }
        else
        {
            ((Component)(object)deliveryDate).gameObject.SetActive(value: false);
        }
    }

    protected virtual void OnSend()
    {
        sendCallback(this);
    }

    protected virtual void OnDeliveryChange(int id, int index)
    {
        RefreshTotal();
    }

    protected virtual void RefreshTotal()
    {
        TotalFragmentWeight = Mathf.FloorToInt((float)(FragmentCount * FactionCtrlr.FragmentPrice) * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT));
        TotalShardWeight = Mathf.FloorToInt((float)(ShardCount * FactionCtrlr.ShardPrice) * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT));
        TotalClusterWeight = Mathf.FloorToInt((float)(ClusterCount * FactionCtrlr.ClusterPrice) * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT));
        TotalWeight = TotalFragmentWeight + TotalShardWeight + TotalClusterWeight;
        sendBtn.SetDisabled(TotalWeight == 0);
    }

    public void ShowArrows(bool show)
    {
        fragDel.SetButtonsVisible(show);
        shardDel.SetButtonsVisible(show);
        clusterDel.SetButtonsVisible(show);
    }
}
