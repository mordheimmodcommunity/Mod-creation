using System;
using UnityEngine;
using UnityEngine.UI;

public class WarbandManagementModule : WarbandSlotPlacementModule
{
    public ToggleEffects swap;

    public ToggleEffects hiredSwords;

    public UIUnitSlot idol;

    public GameObject warbandFees;

    private Action<int, Unit, bool> onUnitSelected;

    private Action<int, Unit, bool> onUnitConfirmed;

    private Action onSwap;

    private Action onHiredSwordsSelected;

    private Action onHiredSwordsConfirmed;

    private bool canHireUnit;

    private bool canHireImpressive;

    public override void Init()
    {
        base.Init();
        if (swap != null)
        {
            swap.onAction.AddListener(delegate
            {
                onSwap();
            });
        }
        hiredSwords.onSelect.AddListener(delegate
        {
            onHiredSwordsSelected();
        });
        hiredSwords.onAction.AddListener(delegate
        {
            onHiredSwordsConfirmed();
        });
    }

    public void Set(Warband warband, Action<int, Unit, bool> unitSelected, Action<int, Unit, bool> unitConfirmed, Action swapUnits = null, Action hiredSwordsSelected = null, Action hiredSwordsConfirmed = null)
    {
        Set(warband, null);
        if (idol != null)
        {
            idol.Set(0, OnUnitSlotOver, OnUnitSlotSelected, OnUnitSlotConfirmed);
            idol.Activate();
            idol.icon.set_sprite(Warband.GetIcon(warband.Id));
            if (warband.GetTotalTreatmentOwned() > 0)
            {
                warbandFees.SetActive(value: true);
                warbandFees.GetComponentInChildren<Text>().set_text(warband.GetTotalTreatmentOwned().ToString());
            }
            else if (warband.GetTotalUpkeepOwned() > 0)
            {
                warbandFees.SetActive(value: true);
                warbandFees.GetComponentInChildren<Text>().set_text(warband.GetTotalUpkeepOwned().ToString());
            }
            else
            {
                warbandFees.SetActive(value: false);
            }
        }
        if (swap != null)
        {
            onSwap = swapUnits;
        }
        onHiredSwordsSelected = hiredSwordsSelected;
        onHiredSwordsConfirmed = hiredSwordsConfirmed;
        onUnitConfirmed = unitConfirmed;
        onUnitSelected = unitSelected;
        currentWarband = warband;
        Refresh();
    }

    public void Refresh()
    {
        canHireUnit = currentWarband.CanHireMoreUnit(isImpressive: false);
        canHireImpressive = currentWarband.CanHireMoreUnit(isImpressive: true);
        SetupAvailableSlots();
    }

    protected override void SetupWarbandSlot(UIUnitSlot slot, Unit unit, int slotIndex, bool isLocked, bool hide = false)
    {
        if (isLocked || hide)
        {
            if (hide)
            {
                slot.gameObject.SetActive(value: false);
                return;
            }
            slot.Set(unit, slotIndex, OnUnitSlotOver, OnUnitSlotSelected, OnUnitSlotConfirmed);
            slot.Activate();
            slot.Lock(lockIcon);
            return;
        }
        slot.Set(unit, slotIndex, OnUnitSlotOver, OnUnitSlotSelected, OnUnitSlotConfirmed);
        if (unit == null && ((slot.isImpressive && !canHireImpressive) || (!slot.isImpressive && !canHireUnit)))
        {
            ((Graphic)slot.icon).set_color(Color.white);
            slot.icon.set_overrideSprite(noneIcon);
        }
        slot.Activate();
    }

    protected override void OnUnitSlotOver(int slotIndex, Unit unit, bool isImpressive)
    {
    }

    protected override void OnUnitSlotSelected(int slotIndex, Unit unit, bool isImpressive)
    {
        onUnitSelected(slotIndex, unit, isImpressive);
    }

    protected override void OnUnitSlotConfirmed(int slotIndex, Unit unit, bool isImpressive)
    {
        onUnitConfirmed(slotIndex, unit, isImpressive);
    }
}
