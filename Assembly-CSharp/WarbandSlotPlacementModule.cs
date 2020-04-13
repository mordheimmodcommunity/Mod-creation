using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class WarbandSlotPlacementModule : UIModule
{
    private SkillsShop skillsShop = new SkillsShop();

    public Sprite lockIcon;

    public Sprite noneIcon;

    public Sprite plusIcon;

    public Sprite swapIcon;

    public UIUnitSlot[] leaderSlots;

    public UIUnitSlot[] impressiveSlots;

    public UIUnitSlot[] heroSlots;

    public UIUnitSlot[] heroImpressiveSlots;

    public UIUnitSlot[] henchmenSlots;

    public UIUnitSlot[] reserveSlots;

    public UIUnitSlot[] reserveImpressiveSlots;

    protected List<UIUnitSlot> allSlots;

    protected List<UIUnitSlot> allImpressiveSlots;

    private WarbandRankSlotData warbandRankSlotData;

    protected Warband currentWarband;

    protected List<int> unitsPosition;

    protected int warbandRatingMin;

    protected int warbandRatingMax;

    private readonly List<int> tempList = new List<int>
    {
        -1,
        -1
    };

    protected bool IsImpressiveAvailable
    {
        get;
        set;
    }

    public override void Init()
    {
        base.Init();
        allSlots = new List<UIUnitSlot>();
        allImpressiveSlots = new List<UIUnitSlot>();
        allSlots.Add(null);
        allSlots.Add(null);
        allSlots.AddRange(leaderSlots);
        allSlots.AddRange(heroSlots);
        allSlots.AddRange(heroImpressiveSlots);
        allSlots.AddRange(henchmenSlots);
        allSlots.AddRange(reserveSlots);
        allImpressiveSlots.AddRange(impressiveSlots);
        allImpressiveSlots.AddRange(reserveImpressiveSlots);
    }

    public void Set(Warband warband, List<int> unitPosition, int ratingMin = 0, int ratingMax = 9999)
    {
        warbandRatingMax = ratingMax;
        warbandRatingMin = ratingMin;
        currentWarband = warband;
        warbandRankSlotData = currentWarband.GetWarbandSlots();
        IsImpressiveAvailable = currentWarband.IsUnitTypeUnlocked(UnitTypeId.IMPRESSIVE);
        unitsPosition = unitPosition;
    }

    public void SetupAvailableSlots()
    {
        SetupSlotSection(leaderSlots, WarbandSlotTypeId.LEADER, warbandRankSlotData.Leader);
        SetupSlotSection(heroSlots, WarbandSlotTypeId.HERO, warbandRankSlotData.Hero);
        SetupImpressiveSlotSection(heroImpressiveSlots, impressiveSlots, WarbandSlotTypeId.HERO_IMPRESSIVE, warbandRankSlotData.Impressive);
        SetupSlotSection(henchmenSlots, WarbandSlotTypeId.HENCHMEN, warbandRankSlotData.Henchman);
        SetupImpressiveSlotSection(reserveSlots, reserveImpressiveSlots, WarbandSlotTypeId.RESERVE, warbandRankSlotData.Reserve);
    }

    protected virtual void SetupWarbandSlot(UIUnitSlot slot, Unit unit, int slotIndex, bool isLocked, bool hide = false)
    {
        if (hide)
        {
            slot.gameObject.SetActive(value: false);
        }
        else if (isLocked)
        {
            slot.Set(unit, slotIndex, OnUnitSlotOver, OnUnitSlotSelected, OnUnitSlotConfirmed, unitsPosition == null);
            slot.Activate();
            slot.Lock(lockIcon);
        }
        else
        {
            slot.Set(unit, slotIndex, OnUnitSlotOver, OnUnitSlotSelected, OnUnitSlotConfirmed, unitsPosition == null);
            slot.Activate();
        }
    }

    private void SetupSlotSection(UIUnitSlot[] slots, WarbandSlotTypeId slotTypeId, int rankSlotAvailable, bool hideEmpty = false)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = (int)(slotTypeId + i);
            Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(slotIndex);
            SetupWarbandSlot(slots[i], unitAtWarbandSlot, slotIndex, rankSlotAvailable == 0 || i >= rankSlotAvailable, hideEmpty && i >= rankSlotAvailable);
        }
    }

    private void SetupImpressiveSlotSection(UIUnitSlot[] slots, UIUnitSlot[] linkedImpressiveSlots, WarbandSlotTypeId slotTypeId, int rankSlotAvailable)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int num = (int)(slotTypeId + i);
            Unit unitAtWarbandSlot = GetUnitAtWarbandSlot(num);
            int num2 = i / 2;
            bool flag = i % 2 == 0;
            if (flag && unitAtWarbandSlot != null && unitAtWarbandSlot.IsImpressive)
            {
                SetupWarbandSlot(linkedImpressiveSlots[num2], unitAtWarbandSlot, num, !IsImpressiveAvailable || i >= rankSlotAvailable);
                SetupWarbandSlot(slots[i], unitAtWarbandSlot, num, isLocked: true);
                slots[i].Deactivate();
                SetupWarbandSlot(slots[i + 1], unitAtWarbandSlot, num, isLocked: true);
                slots[i + 1].Deactivate();
                i++;
                continue;
            }
            SetupWarbandSlot(slots[i], unitAtWarbandSlot, num, rankSlotAvailable == 0 || i >= rankSlotAvailable);
            if (flag)
            {
                SetupWarbandSlot(linkedImpressiveSlots[num2], null, num, !IsImpressiveAvailable || i + 1 >= rankSlotAvailable);
                if (unitAtWarbandSlot != null || GetUnitAtWarbandSlot(num + 1) != null)
                {
                    linkedImpressiveSlots[num2].icon.set_overrideSprite(lockIcon);
                    ((Graphic)linkedImpressiveSlots[num2].icon).set_color(Color.white);
                    linkedImpressiveSlots[num2].Deactivate();
                }
            }
        }
    }

    protected Unit GetUnitAtWarbandSlot(int slotIndex)
    {
        if (unitsPosition != null)
        {
            int num = unitsPosition.IndexOf(slotIndex);
            if (num == -1)
            {
                return null;
            }
            return currentWarband.Units[num];
        }
        return currentWarband.GetUnitAtWarbandSlot(slotIndex, includeUnavailable: true);
    }

    protected int GetUnitSlotIndex(Unit unit)
    {
        if (unitsPosition != null)
        {
            int index = currentWarband.Units.IndexOf(unit);
            return unitsPosition[index];
        }
        return unit.UnitSave.warbandSlotIndex;
    }

    protected void SetUnitSlotIndex(Unit unit, int slotIndex, bool checkIfAvailable = false)
    {
        if (unitsPosition != null)
        {
            int index = currentWarband.Units.IndexOf(unit);
            unitsPosition[index] = slotIndex;
        }
        else
        {
            unit.UnitSave.warbandSlotIndex = slotIndex;
        }
    }

    protected int GetActiveUnitIdCount(UnitId unitId, List<int> excludeSlots = null)
    {
        if (unitsPosition != null)
        {
            int num = 0;
            for (int i = 0; i < unitsPosition.Count; i++)
            {
                if (currentWarband.IsActiveWarbandSlot(unitsPosition[i]) && currentWarband.Units[i].Id == unitId && (excludeSlots == null || !excludeSlots.Contains(unitsPosition[i])))
                {
                    num++;
                }
            }
            return num;
        }
        return currentWarband.GetActiveUnitIdCount(unitId, excludeSlots);
    }

    protected bool IsUnitCountExceeded(Unit unit, int excludeSlot1, int excludeSlot2)
    {
        tempList[0] = excludeSlot1;
        tempList[1] = excludeSlot2;
        return GetActiveUnitIdCount(unit.Id, tempList) >= unit.Data.MaxCount;
    }

    protected bool CanPlaceUnitAt(Unit unit, int toIndex)
    {
        if (unitsPosition == null && unit.GetActiveStatus() == UnitActiveStatusId.IN_TRAINING && skillsShop.IsSkillChangeType(unit.UnitSave.skillInTrainingId) && toIndex < 12)
        {
            return false;
        }
        return currentWarband.CanPlaceUnitAt(unit, toIndex) && (!currentWarband.IsActiveWarbandSlot(toIndex) || !IsUnitCountExceeded(unit, unit.UnitSave.warbandSlotIndex, toIndex));
    }

    protected int GetActiveUnitsCount()
    {
        if (unitsPosition != null)
        {
            int num = 0;
            for (int i = 0; i < unitsPosition.Count; i++)
            {
                if (currentWarband.IsActiveWarbandSlot(unitsPosition[i]))
                {
                    num++;
                }
            }
            return num;
        }
        return currentWarband.GetNbActiveUnits(impressiveCountFor2: false);
    }

    protected int FindEmptyAvailableSlot()
    {
        int num = currentWarband.GetNbMaxActiveSlots() + currentWarband.GetNbMaxReserveSlot();
        for (int i = 0; i < num; i++)
        {
            if (!unitsPosition.Contains(20 + i))
            {
                return 20 + i;
            }
        }
        return -1;
    }

    protected abstract void OnUnitSlotOver(int slotIndex, Unit unit, bool isImpressive);

    protected abstract void OnUnitSlotSelected(int slotIndex, Unit unit, bool isImpressive);

    protected abstract void OnUnitSlotConfirmed(int slotIndex, Unit unit, bool isImpressive);
}
