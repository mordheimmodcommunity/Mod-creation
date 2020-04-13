using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HideoutInventory : BaseHideoutUnitState
{
    private InventoryModule inventoryModule;

    private WheelModule wheelModule;

    private ItemDescModule itemDescModule;

    private UnitSlotId wheelSelectedSlotId;

    private RuneMark currentRune;

    private Item tempItem;

    private TreasuryModule treasuryMod;

    private Warband warband;

    private List<RuneMark> availableRuneMarks = new List<RuneMark>();

    private List<RuneMark> notAvailableRuneMarks = new List<RuneMark>();

    public HideoutInventory(HideoutManager mng, HideoutCamAnchor anchor)
        : base(anchor, HideoutManager.State.INVENTORY)
    {
    }

    public override void Enter(int iFrom)
    {
        warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.INVENTORY);
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count > 1)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WHEEL, ModuleId.ITEM_DESC, ModuleId.DESC, ModuleId.NEXT_UNIT, ModuleId.TITLE, ModuleId.UNIT_TABS, ModuleId.CHARACTER_AREA, ModuleId.NOTIFICATION);
            PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<NextUnitModule>(ModuleId.NEXT_UNIT).Setup();
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WHEEL, ModuleId.ITEM_DESC, ModuleId.DESC, ModuleId.TITLE, ModuleId.UNIT_TABS, ModuleId.CHARACTER_AREA, ModuleId.NOTIFICATION);
        }
        base.Enter(iFrom);
        treasuryMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
        treasuryMod.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
        inventoryModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<InventoryModule>(ModuleId.INVENTORY);
        inventoryModule.Init(OnInventoryTabChanged);
        wheelModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WheelModule>(ModuleId.WHEEL);
        wheelModule.Activate(ModuleCentertOnLeft(), OnWheelSlotSelected, OnWheelMutationSlotSelected, OnWheelSlotConfirmed, OnWheelMutationSlotConfirmed);
        itemDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<ItemDescModule>(ModuleId.ITEM_DESC);
        descModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<DescriptionModule>(ModuleId.DESC);
        descModule.gameObject.SetActive(value: false);
        characterCamModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CharacterCameraAreaModule>(ModuleId.CHARACTER_AREA);
        characterCamModule.Init(camAnchor.transform.position);
        currentRune = null;
        tempItem = null;
        SelectUnit(PandoraSingleton<HideoutManager>.Instance.currentUnit);
        inventoryModule.SetTab(InventoryModuleTab.INVENTORY, sendCallback: false);
        inventoryModule.Clear();
        wheelModule.itemSlots[2].SetSelected(force: true);
        SetButtonsForWheelSelection();
    }

    public override void Exit(int iTo)
    {
        base.Exit(iTo);
        inventoryModule.Clear();
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WheelModule>(ModuleId.WHEEL).Deactivate();
    }

    private void OnUnitChanged()
    {
        SelectUnit(PandoraSingleton<HideoutManager>.Instance.currentUnit);
    }

    public override void SelectUnit(UnitMenuController ctrlr)
    {
        base.SelectUnit(ctrlr);
        wheelSelectedSlotId = UnitSlotId.SET1_MAINHAND;
        wheelModule.itemSlots[2].SetSelected(force: true);
        RefreshStatsAndSlots();
        OnWheelSlotConfirmed(wheelSelectedSlotId);
    }

    public override Selectable ModuleLeftOnRight()
    {
        return (Selectable)(object)wheelModule.itemSlots[0].slot.toggle;
    }

    private void OnWheelSlotSelected(UnitSlotId slotId)
    {
        SetSlotDescription(slotId);
    }

    private void OnWheelSlotConfirmed(UnitSlotId slotId)
    {
        PandoraDebug.LogDebug("Wheel slot selected " + slotId);
        if ((slotId == UnitSlotId.SET1_OFFHAND || slotId == UnitSlotId.SET2_OFFHAND) && (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.Items[(int)(slotId - 1)].IsPaired || PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.Items[(int)(slotId - 1)].IsTwoHanded))
        {
            inventoryModule.Clear();
            wheelSelectedSlotId = UnitSlotId.NB_SLOTS;
        }
        else
        {
            wheelSelectedSlotId = slotId;
            inventoryModule.SetTab(InventoryModuleTab.INVENTORY, sendCallback: false);
            SetActiveSlot(setList: true);
        }
        if (Input.mousePresent)
        {
            PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
            PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: true);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: false);
        }
        PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("cancel", "menu_return_select_slot");
        PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(ReturnToWheelSlotSelection, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.SetAction("action", "menu_equip");
        PandoraSingleton<HideoutTabManager>.Instance.button3.OnAction(null, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
    }

    private void OnWheelMutationSlotSelected(int mutationIdx)
    {
        if (mutationIdx != -1)
        {
            PandoraDebug.LogInfo("Wheel mutation slot confirmed (unitsMutationIdx : " + mutationIdx + ")");
            inventoryModule.Clear();
            wheelSelectedSlotId = UnitSlotId.NB_SLOTS;
            descModule.gameObject.SetActive(value: false);
            itemDescModule.gameObject.SetActive(value: true);
            itemDescModule.SetMutation(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.Mutations[mutationIdx]);
        }
    }

    private void OnWheelMutationSlotConfirmed(int mutationIdx)
    {
        OnWheelMutationSlotSelected(mutationIdx);
        if (mutationIdx != -1)
        {
            SetButtonsForWheelSelection();
        }
    }

    private void ReturnToWheelSlotSelection()
    {
        wheelModule.Unlock();
        itemDescModule.HideDesc(1);
        inventoryModule.Clear();
        if (wheelSelectedSlotId != UnitSlotId.NONE && wheelSelectedSlotId < UnitSlotId.NB_SLOTS)
        {
            wheelModule.itemSlots[(int)wheelSelectedSlotId].SetSelected(force: true);
        }
        else
        {
            wheelModule.itemSlots[2].SetSelected(force: true);
        }
        SetButtonsForWheelSelection();
    }

    private void OnInventoryTabChanged(InventoryModuleTab tab)
    {
        SetActiveSlot(setList: true);
    }

    private void OnInventorySlotConfirmed(Item item)
    {
        if (wheelSelectedSlotId == UnitSlotId.NB_SLOTS)
        {
            return;
        }
        List<ItemConsumableData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemConsumableData>(new string[2]
        {
            "fk_item_id",
            "fk_item_quality_id"
        }, new string[2]
        {
            ((int)item.Id).ToString(),
            ((int)item.QualityData.Id).ToString()
        });
        tempItem = item;
        if (inventoryModule.currentTab == InventoryModuleTab.SHOP)
        {
            if (warband.GetItemBuyPrice(tempItem) > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
            {
                PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_item"), null);
                PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
            }
            else
            {
                PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_buy_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_buy_confirm_desc", tempItem.LocalizedName, warband.GetItemBuyPrice(tempItem).ToString()), OnInventoryItemPopupConfirmed);
            }
        }
        else if (list != null && list.Count > 0 && list[0].OutOfCombat)
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_consumable_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_consumable_confirm_desc", tempItem.LocalizedName), OnInventoryItemPopupConfirmed);
        }
        else
        {
            OnInventoryItemPopupConfirmed(confirm: true);
        }
    }

    private void OnInventoryItemPopupConfirmed(bool confirm)
    {
        if (confirm)
        {
            ItemSave itemSave = null;
            if (tempItem.Id == ItemId.NONE)
            {
                itemSave = new ItemSave(ItemId.NONE);
            }
            else if (inventoryModule.currentTab == InventoryModuleTab.SHOP)
            {
                itemSave = PandoraSingleton<HideoutManager>.Instance.Market.PopItem(tempItem.Save);
                int itemBuyPrice = warband.GetItemBuyPrice(tempItem);
                Pay(itemBuyPrice);
                warband.AddToAttribute(WarbandAttributeId.BUY_AMOUNT, itemBuyPrice);
            }
            else
            {
                itemSave = PandoraSingleton<HideoutManager>.Instance.WarbandChest.PopItem(tempItem.Save);
            }
            List<ItemSave> list = PandoraSingleton<HideoutManager>.Instance.currentUnit.EquipItem(wheelSelectedSlotId, itemSave);
            PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.CheckItemsAchievments();
            List<Item> list2 = new List<Item>();
            PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UpdateAttributesAndCheckBackPack(list2);
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list2);
            PandoraSingleton<GameManager>.Instance.Profile.CheckEquipAchievement(currentUnit.unit, wheelSelectedSlotId);
            for (int i = 0; i < list.Count; i++)
            {
                PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(list[i]);
            }
            SaveChanges();
            PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UpdateAttributes();
            RefreshStatsAndSlots();
            ReturnToWheelSlotSelection();
        }
        tempItem = null;
    }

    private void OnInventorySlotSelected(Item item)
    {
        descModule.gameObject.SetActive(value: false);
        itemDescModule.gameObject.SetActive(value: true);
        if (item.Id == ItemId.NONE)
        {
            itemDescModule.HideDesc(1);
            if (currentUnit.unit.Items[(int)wheelSelectedSlotId].RuneMark != null)
            {
                itemDescModule.SetRune(currentUnit.unit.Items[(int)wheelSelectedSlotId].RuneMark);
            }
        }
        else
        {
            itemDescModule.SetItem(item, wheelSelectedSlotId, 1);
        }
    }

    private void OnInventorySlotConfirmed(RuneMark rune)
    {
        if (wheelSelectedSlotId != UnitSlotId.NB_SLOTS)
        {
            if (warband.GetRuneMarkBuyPrice(rune) > PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold())
            {
                PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_gold_rune", rune.FullLocName), null);
                PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
            }
            else
            {
                currentRune = rune;
                UnitMenuController currentUnit = PandoraSingleton<HideoutManager>.Instance.currentUnit;
                PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rune_confirm_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_rune_confirm_desc", rune.FullLocName, currentUnit.unit.Items[(int)wheelSelectedSlotId].LocalizedName), OnInventoryRunePopupConfirmed);
            }
        }
    }

    private void OnInventoryRunePopupConfirmed(bool isConfirm)
    {
        if (isConfirm)
        {
            UnitMenuController currentUnit = PandoraSingleton<HideoutManager>.Instance.currentUnit;
            Pay(warband.GetRuneMarkBuyPrice(currentRune));
            currentUnit.unit.Items[(int)wheelSelectedSlotId].AddRuneMark(currentRune.Data.Id, currentRune.QualityData.Id, currentUnit.unit.AllegianceId);
            currentUnit.unit.CheckItemsAchievments();
            List<Item> list = new List<Item>();
            PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UpdateAttributesAndCheckBackPack(list);
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItems(list);
            SaveChanges();
            PandoraSingleton<GameManager>.Instance.Profile.CheckEquipAchievement(base.currentUnit.unit, wheelSelectedSlotId);
            RefreshStatsAndSlots();
            ReturnToWheelSlotSelection();
        }
        currentRune = null;
    }

    private void OnInventorySlotSelected(RuneMark rune)
    {
        descModule.gameObject.SetActive(value: false);
        itemDescModule.gameObject.SetActive(value: true);
        itemDescModule.SetItem(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.Items[(int)wheelSelectedSlotId], wheelSelectedSlotId, 0);
        if (rune == null)
        {
            itemDescModule.HideDesc(1);
            if (currentUnit.unit.Items[(int)wheelSelectedSlotId].RuneMark != null)
            {
                itemDescModule.SetRune(currentUnit.unit.Items[(int)wheelSelectedSlotId].RuneMark);
            }
        }
        else
        {
            itemDescModule.SetRune(rune);
        }
    }

    private void SetButtonsForWheelSelection()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "menu_select_slot");
        PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(null, mouseOnly: false);
        SetupApplyButton(PandoraSingleton<HideoutTabManager>.Instance.button3);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
    }

    private void RefreshStatsAndSlots()
    {
        RefreshUnitAttributes();
        SetActiveSlot(setList: false);
        wheelModule.RefreshSlots(currentUnit);
    }

    private void SetSlotDescription(UnitSlotId slotId)
    {
        descModule.gameObject.SetActive(value: false);
        itemDescModule.gameObject.SetActive(value: true);
        if (currentUnit != null && (int)slotId < currentUnit.unit.Items.Count && currentUnit.unit.Items[(int)slotId].Id != 0)
        {
            itemDescModule.SetItem(currentUnit.unit.Items[(int)slotId], slotId, 0);
        }
    }

    private void SetActiveSlot(bool setList)
    {
        if (wheelSelectedSlotId == UnitSlotId.NB_SLOTS || (int)wheelSelectedSlotId >= currentUnit.unit.Items.Count)
        {
            wheelSelectedSlotId = UnitSlotId.SET1_MAINHAND;
        }
        descModule.gameObject.SetActive(value: false);
        itemDescModule.gameObject.SetActive(value: true);
        itemDescModule.HideDesc(1);
        UnitSlotId nextWeaponSlot = (wheelSelectedSlotId != UnitSlotId.SET2_MAINHAND && wheelSelectedSlotId != UnitSlotId.SET2_OFFHAND) ? UnitSlotId.SET1_MAINHAND : UnitSlotId.SET2_MAINHAND;
        currentUnit.SwitchWeapons(nextWeaponSlot);
        RefreshUnitAttributes();
        UnitSlotId slotId = wheelSelectedSlotId;
        if (currentUnit.unit.Items[(int)wheelSelectedSlotId].Id != 0)
        {
            itemDescModule.SetItem(currentUnit.unit.Items[(int)wheelSelectedSlotId], wheelSelectedSlotId, 0);
        }
        else
        {
            itemDescModule.HideDesc(0);
        }
        EventSystem.get_current().SetSelectedGameObject((GameObject)null);
        if (setList)
        {
            switch (inventoryModule.currentTab)
            {
                case InventoryModuleTab.INVENTORY:
                    {
                        List<ItemSave> items2 = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItems(currentUnit.unit, slotId);
                        string reason3 = (items2.Count <= 0) ? "na_item_inventory" : string.Empty;
                        SetInventoryModuleItems(items2, slotId, reason3);
                        wheelModule.Lock();
                        break;
                    }
                case InventoryModuleTab.SHOP:
                    {
                        List<ItemSave> items = PandoraSingleton<HideoutManager>.Instance.Market.GetItems(currentUnit.unit, slotId);
                        string reason2 = (items.Count <= 0) ? "na_item_store" : string.Empty;
                        SetInventoryModuleItems(items, slotId, reason2);
                        wheelModule.Lock();
                        break;
                    }
                case InventoryModuleTab.ENCHANTS:
                    {
                        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetAvailableRuneMarks(wheelSelectedSlotId, currentUnit.unit.Items[(int)wheelSelectedSlotId], out string reason, ref availableRuneMarks, ref notAvailableRuneMarks);
                        inventoryModule.SetList(availableRuneMarks, notAvailableRuneMarks, OnInventorySlotConfirmed, OnInventorySlotSelected, reason);
                        wheelModule.Lock();
                        break;
                    }
            }
        }
    }

    private void SetInventoryModuleItems(List<ItemSave> itemSaves, UnitSlotId slotId, string reason)
    {
        List<Item> list = new List<Item>();
        for (int i = 0; i < itemSaves.Count; i++)
        {
            Item item = new Item(itemSaves[i]);
            item.SetModifiers(currentUnit.unit.GetMutationId(slotId));
            list.Add(item);
        }
        inventoryModule.SetList(list, OnInventorySlotConfirmed, OnInventorySlotSelected, wheelSelectedSlotId, reason, currentUnit.unit.Items[(int)slotId].IsLockSlot);
    }

    protected override void ShowDescription(string title, string desc)
    {
        base.ShowDescription(title, desc);
        itemDescModule.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
        SetupAttributeButtons(PandoraSingleton<HideoutTabManager>.Instance.button2, PandoraSingleton<HideoutTabManager>.Instance.button3, PandoraSingleton<HideoutTabManager>.Instance.button4);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
    }

    public override void OnApplyChanges()
    {
        base.OnApplyChanges();
        ReturnToWheelSlotSelection();
        wheelModule.RefreshSlots(currentUnit);
    }

    protected override void OnAttributeChanged()
    {
        base.OnAttributeChanged();
        inventoryModule.Clear();
    }

    private void Pay(int amount)
    {
        PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(amount);
        PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.SHOP_GOLD, amount);
        treasuryMod.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
    }

    private void SaveChanges()
    {
        PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.UpdateAttributes();
        RefreshUnitAttributes();
        ReturnToWheelSlotSelection();
    }

    public override bool CanIncreaseAttributes()
    {
        return true;
    }
}
