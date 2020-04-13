using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmugglerFactionShipmentModule : UIModule
{
    public Text factionType;

    public Text shipmentType;

    public Text reputation;

    public Text progression;

    public Slider progressBar;

    public Text description;

    public FactionShipment shipment;

    public FactionDelivery delivery;

    private FactionShipment currentFactionShipment;

    private Action onShipmentSent;

    public void Setup(FactionMenuController faction, Action onShipmentSentCb)
    {
        onShipmentSent = onShipmentSentCb;
        description.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(faction.Faction.Data.Desc + "_desc"));
        factionType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(faction.Faction.Data.Desc + "_name"));
        reputation.set_text(faction.Faction.Save.rank.ToString());
        if (faction.NextRankReputation == -1)
        {
            progression.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_max_rank"));
            progressBar.set_normalizedValue(1f);
        }
        else
        {
            progression.set_text($"{faction.Faction.Reputation}/{faction.NextRankReputation}");
            progressBar.set_normalizedValue((float)faction.Faction.Reputation / (float)faction.NextRankReputation);
        }
        if (faction.Faction.Primary && faction.HasShipment)
        {
            shipmentType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_fill_request"));
            delivery.gameObject.SetActive(value: false);
            shipment.gameObject.SetActive(value: true);
            shipment.SetFaction(faction, OnSendShipment);
        }
        else
        {
            shipmentType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_send"));
            delivery.gameObject.SetActive(value: true);
            shipment.gameObject.SetActive(value: false);
            delivery.SetFaction(faction, OnSendShipment);
        }
    }

    public void SetFocus()
    {
        if (shipment.gameObject.activeInHierarchy)
        {
            shipment.fragGroup.SetSelected();
        }
        else
        {
            delivery.fragGroup.SetSelected();
        }
    }

    public void OnLostFocus()
    {
        shipment.toggleGroup.SetAllTogglesOff();
        delivery.toggleGroup.SetAllTogglesOff();
    }

    private void OnSendShipment(FactionShipment factionShipment)
    {
        if (factionShipment.TotalWeight <= 0)
        {
            return;
        }
        currentFactionShipment = factionShipment;
        int num = (!currentFactionShipment.FactionCtrlr.Faction.Primary || !currentFactionShipment.FactionCtrlr.HasShipment) ? currentFactionShipment.TotalWeight : (currentFactionShipment.TotalWeight - currentFactionShipment.FactionCtrlr.ShipmentWeight);
        int num2 = num - currentFactionShipment.FactionCtrlr.MaxReputationGain;
        if (num2 > 0)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.MEDIUM, PopupModuleId.POPUP_GENERIC_ANCHOR);
            List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[1]
            {
                PopupModuleId.POPUP_GENERIC_ANCHOR
            });
            ConfirmationPopupView confirmationPopupView = (ConfirmationPopupView)modulesPopup[0];
            if (currentFactionShipment.FactionCtrlr.MaxReputationGain > 0)
            {
                confirmationPopupView.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_reputation_warning_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_reputation_warning_desc", currentFactionShipment.FactionCtrlr.MaxReputationGain.ToString()), delegate (bool confirm)
                {
                    if (confirm)
                    {
                        DoSendShipment();
                    }
                });
            }
            else if (!currentFactionShipment.FactionCtrlr.WarSave.smugglersMaxRankShown)
            {
                currentFactionShipment.FactionCtrlr.WarSave.smugglersMaxRankShown = true;
                confirmationPopupView.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_reputation_max_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_reputation_max_desc"), delegate (bool confirm)
                {
                    if (confirm)
                    {
                        DoSendShipment();
                    }
                });
                confirmationPopupView.HideCancelButton();
            }
            else
            {
                DoSendShipment();
            }
        }
        else
        {
            DoSendShipment();
        }
    }

    private void DoSendShipment()
    {
        if (currentFactionShipment.FactionCtrlr.Faction.Primary && currentFactionShipment.FactionCtrlr.HasShipment)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.MEDIUM, PopupModuleId.POPUP_GENERIC_ANCHOR, PopupModuleId.POPUP_SEND_SHIPMENT_REQUEST);
            List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[2]
            {
                PopupModuleId.POPUP_GENERIC_ANCHOR,
                PopupModuleId.POPUP_SEND_SHIPMENT_REQUEST
            });
            SendShipmentRequestPopup sendShipmentRequestPopup = (SendShipmentRequestPopup)modulesPopup[1];
            sendShipmentRequestPopup.Set((FactionRequest)currentFactionShipment);
            ConfirmationPopupView confirmationPopupView = (ConfirmationPopupView)modulesPopup[0];
            confirmationPopupView.Show("popup_request_title", "popup_request_desc", OnRequestConfirm);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.MEDIUM, PopupModuleId.POPUP_GENERIC_ANCHOR, PopupModuleId.POPUP_SEND_SHIPMENT);
            List<UIPopupModule> modulesPopup2 = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[2]
            {
                PopupModuleId.POPUP_GENERIC_ANCHOR,
                PopupModuleId.POPUP_SEND_SHIPMENT
            });
            SendShipmentPopup sendShipmentPopup = (SendShipmentPopup)modulesPopup2[1];
            sendShipmentPopup.Set((FactionDelivery)currentFactionShipment);
            ConfirmationPopupView confirmationPopupView2 = (ConfirmationPopupView)modulesPopup2[0];
            confirmationPopupView2.Show("popup_shipment_title", "popup_shipment_desc", OnShipmentConfirm);
        }
    }

    private void OnShipmentConfirm(bool confirm)
    {
        if (confirm)
        {
            RemoveWyrdstone();
            int deliveryDate = GetDeliveryDate(currentFactionShipment.FactionCtrlr.Faction.GetFactionDeliveryEvent());
            SendShipment(currentFactionShipment.FactionCtrlr.Faction.GetFactionDeliveryEvent(), deliveryDate, currentFactionShipment.TotalWeight, currentFactionShipment.FactionCtrlr.GetDeliveryPrice(currentFactionShipment.TotalWeight), currentFactionShipment.FactionCtrlr.GetDeliveryReputation(currentFactionShipment.TotalWeight));
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
            onShipmentSent();
        }
    }

    private void OnRequestConfirm(bool confirm)
    {
        Warband warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        WarbandSave warbandSave = warband.GetWarbandSave();
        EventLogger logger = warband.Logger;
        if (confirm)
        {
            RemoveWyrdstone();
            int currentDate = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
            int deliveryDate = GetDeliveryDate(currentFactionShipment.FactionCtrlr.Faction.GetFactionDeliveryEvent());
            int num = currentFactionShipment.TotalWeight - currentFactionShipment.FactionCtrlr.ShipmentWeight;
            SendShipment(EventLogger.LogEvent.SHIPMENT_DELIVERY, deliveryDate, currentFactionShipment.FactionCtrlr.ShipmentWeight, currentFactionShipment.FactionCtrlr.ShipmentGoldReward, 0);
            warband.AddToAttribute(WarbandAttributeId.DELIVERY_WITHOUT_DELAY, 1);
            SendShipment(currentFactionShipment.FactionCtrlr.Faction.GetFactionDeliveryEvent(), deliveryDate, num, currentFactionShipment.FactionCtrlr.GetDeliveryPrice(num), currentFactionShipment.FactionCtrlr.GetDeliveryReputation(currentFactionShipment.TotalWeight));
            Tuple<int, EventLogger.LogEvent, int> tuple = logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
            warbandSave.nextShipmentExtraDays = tuple.Item1 - currentDate;
            warbandSave.lastShipmentFailed = false;
            logger.RemoveHistory(tuple);
            warband.UpdateAttributes();
            currentFactionShipment.FactionCtrlr.CreateNewShipmentRequest(tuple.Item1, deliveryDate);
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
            onShipmentSent();
        }
    }

    private int GetDeliveryDate(EventLogger.LogEvent log)
    {
        int currentDate = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
        Tuple<int, EventLogger.LogEvent, int> tuple = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.FindLastEvent(log);
        int num = currentDate + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(currentFactionShipment.FactionCtrlr.Faction.Data.MinDeliveryDays, currentFactionShipment.FactionCtrlr.Faction.Data.MaxDeliveryDays + 1);
        if (tuple != null && tuple.Item1 > currentDate)
        {
            num = Mathf.Max(num, tuple.Item1 + 1);
        }
        return num;
    }

    private void SendShipment(EventLogger.LogEvent logEvent, int date, int weight, int gold, int reputation)
    {
        int rank = currentFactionShipment.FactionCtrlr.AddReputation(reputation);
        int id = gold;
        bool flag = true;
        if (logEvent != EventLogger.LogEvent.SHIPMENT_DELIVERY)
        {
            flag = currentFactionShipment.FactionCtrlr.Faction.SaveDelivery(weight, gold, rank, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, out id);
        }
        if (flag)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger.AddHistory(date, logEvent, id);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.MEDIUM, PopupModuleId.POPUP_GENERIC_ANCHOR);
            List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[1]
            {
                PopupModuleId.POPUP_GENERIC_ANCHOR
            });
            ConfirmationPopupView confirmationPopupView = (ConfirmationPopupView)modulesPopup[0];
            confirmationPopupView.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_multiple_shipments_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("smuggler_multiple_shipments_desc", currentFactionShipment.FactionCtrlr.MaxReputationGain.ToString()), null);
            confirmationPopupView.HideCancelButton();
        }
        if (date == PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            PandoraSingleton<HideoutManager>.Instance.Progressor.DoDelivery(currentFactionShipment.FactionCtrlr, id, logEvent == EventLogger.LogEvent.SHIPMENT_DELIVERY);
        }
    }

    private void RemoveWyrdstone()
    {
        WarbandChest warbandChest = PandoraSingleton<HideoutManager>.Instance.WarbandChest;
        warbandChest.RemoveItem(ItemId.WYRDSTONE_FRAGMENT, currentFactionShipment.FragmentCount);
        warbandChest.RemoveItem(ItemId.WYRDSTONE_SHARD, currentFactionShipment.ShardCount);
        warbandChest.RemoveItem(ItemId.WYRDSTONE_CLUSTER, currentFactionShipment.ClusterCount);
        TreasuryModule moduleRight = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
        moduleRight.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
    }
}
