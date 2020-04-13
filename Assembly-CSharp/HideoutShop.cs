using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideoutShop : ICheapState
{
    private HideoutCamAnchor camAnchor;

    private ShopModule shopModule;

    private ItemDescModule descModule;

    private Item tempItem;

    private LatestArrivalModule latestMod;

    private TreasuryModule treasuryMod;

    private Warband warband;

    private WarbandTabsModule warbandTabs;

    private bool once = true;

    public HideoutShop(HideoutManager mng, HideoutCamAnchor anchor)
    {
        camAnchor = anchor;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
        PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.LATEST_ARRIVAL);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.SHOP);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.ITEM_DESC, ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
        warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
        warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
        warbandTabs.SetCurrentTab(HideoutManager.State.SHOP);
        warbandTabs.Refresh();
        latestMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<LatestArrivalModule>(ModuleId.LATEST_ARRIVAL);
        List<ItemSave> addedItems = PandoraSingleton<HideoutManager>.Instance.Market.GetAddedItems();
        latestMod.Set(PandoraSingleton<HideoutManager>.Instance.Market.CurrentEventId, addedItems, null, null);
        treasuryMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
        descModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<ItemDescModule>(ModuleId.ITEM_DESC);
        descModule.HideAll();
        shopModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<ShopModule>(ModuleId.SHOP);
        shopModule.Init(OnTabSelected);
        shopModule.SetTab(ShopModuleTab.BUY);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ReturnToCamp, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
        GameObject shopNodeContent = PandoraSingleton<HideoutManager>.Instance.GetShopNodeContent();
        PandoraSingleton<HideoutManager>.Instance.shopNode.SetContent(shopNodeContent);
        shopNodeContent.transform.localPosition = new Vector3(0.84f, 0f, 0f);
        shopNodeContent.transform.localRotation = Quaternion.Euler(new Vector3(0f, 340f, 0f));
        once = true;
    }

    void ICheapState.Exit(int iTo)
    {
        shopModule.itemRuneList.Clear();
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
        if (once)
        {
            once = false;
            PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.SHOP);
        }
    }

    private void ReturnToCamp()
    {
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
    }

    private void BuySell()
    {
        if (tempItem != null)
        {
            OnSlotConfirmed(tempItem);
        }
    }

    private void OnTabSelected(ShopModuleTab tab)
    {
        List<ItemSave> list = null;
        switch (tab)
        {
            case ShopModuleTab.BUY:
                list = PandoraSingleton<HideoutManager>.Instance.Market.GetItems();
                break;
            case ShopModuleTab.SELL:
                list = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetSellableItems();
                break;
        }
        List<Item> list2 = new List<Item>();
        for (int i = 0; i < list.Count; i++)
        {
            Item item = new Item(list[i]);
            list2.Add(item);
        }
        list2.Sort(new CompareItem());
        shopModule.SetList(list2, OnSlotConfirmed, OnSlotSelected, tab == ShopModuleTab.BUY);
        if (list2.Count <= 0)
        {
            descModule.HideAll();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
        {
            shopModule.StartCoroutine(SelectFirstItemOnNextFrame());
        }
    }

    private IEnumerator SelectFirstItemOnNextFrame()
    {
        yield return null;
        shopModule.itemRuneList.scrollGroup.items[0].SetSelected();
    }

    private void OnSlotSelected(Item item)
    {
        tempItem = item;
        descModule.SetItem(item, UnitSlotId.NONE, 0);
    }

    private void OnSlotConfirmed(Item item)
    {
        if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 0)
        {
            return;
        }
        tempItem = item;
        switch (shopModule.currentTab)
        {
            case ShopModuleTab.BUY:
                {
                    if (!PandoraSingleton<HideoutManager>.Instance.Market.HasItem(tempItem))
                    {
                        ItemNotAvailalble();
                        break;
                    }
                    if (warband.GetItemBuyPrice(tempItem) > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
                    {
                        PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_item"), null);
                        PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
                        break;
                    }
                    int itemBuyPrice = warband.GetItemBuyPrice(tempItem);
                    PandoraSingleton<HideoutManager>.Instance.shopConfirmPopup.Show(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_buy_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_buy_confirm_desc", tempItem.LocalizedName, itemBuyPrice.ToString()), tempItem.Amount, itemBuyPrice, checkFunds: true, OnBuyItemPopupConfirmed);
                    PandoraSingleton<HideoutManager>.Instance.shopConfirmPopup.confirmButton.SetSelected();
                    PandoraSingleton<HideoutManager>.Instance.shopConfirmPopup.confirmButton.effects.toggle.set_isOn(true);
                    break;
                }
            case ShopModuleTab.SELL:
                {
                    if (!PandoraSingleton<HideoutManager>.Instance.WarbandChest.HasItem(tempItem))
                    {
                        ItemNotAvailalble();
                        break;
                    }
                    int itemSellPrice = warband.GetItemSellPrice(tempItem);
                    PandoraSingleton<HideoutManager>.Instance.shopConfirmPopup.Show(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_sell_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_sell_confirm_desc", tempItem.LocalizedName, itemSellPrice.ToString()), tempItem.Amount, itemSellPrice, checkFunds: false, OnSellItemPopupConfirmed);
                    break;
                }
        }
    }

    private void OnBuyItemPopupConfirmed(bool confirm, int qty)
    {
        if (confirm && tempItem != null)
        {
            int itemBuyPrice = warband.GetItemBuyPrice(tempItem);
            if (itemBuyPrice * qty <= PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
            {
                ItemSave save = PandoraSingleton<HideoutManager>.Instance.Market.PopItem(tempItem.Save, qty);
                PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(qty * itemBuyPrice);
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.SHOP_GOLD, qty * itemBuyPrice);
                PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(save);
                warband.AddToAttribute(WarbandAttributeId.BUY_AMOUNT, qty * itemBuyPrice);
                SaveRefresh();
            }
        }
        tempItem = null;
    }

    private void OnSellItemPopupConfirmed(bool confirm, int qty)
    {
        if (confirm && tempItem != null)
        {
            ItemSave save = PandoraSingleton<HideoutManager>.Instance.WarbandChest.PopItem(tempItem.Save, qty);
            int itemSellPrice = warband.GetItemSellPrice(tempItem);
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddGold(qty * itemSellPrice);
            PandoraSingleton<HideoutManager>.Instance.Market.AddSoldItem(save);
            warband.AddToAttribute(WarbandAttributeId.SELL_AMOUNT, qty * itemSellPrice);
            SaveRefresh();
        }
        tempItem = null;
    }

    private void SaveRefresh()
    {
        PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        latestMod.Set(PandoraSingleton<HideoutManager>.Instance.Market.CurrentEventId, PandoraSingleton<HideoutManager>.Instance.Market.GetAddedItems(), null, null);
        shopModule.RemoveItem(tempItem);
        treasuryMod.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
    }

    private void ItemNotAvailalble()
    {
        PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_unavailable"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_desc_unavailable", tempItem.LocalizedName), null);
        PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
    }
}
