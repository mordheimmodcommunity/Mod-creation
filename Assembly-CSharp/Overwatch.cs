using UnityEngine;

public class Overwatch : ICheapState
{
    private UnitController unitCtrlr;

    private float minDistance;

    private float distance;

    private TargetData targetData;

    private float requiredPerc;

    public Overwatch(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        requiredPerc = Constant.GetFloat(ConstantId.RANGE_SHOOT_REQUIRED_PERC);
        minDistance = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMin;
        distance = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMax - 2;
        if (unitCtrlr.GetCurrentShots() == 0)
        {
            unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Reload();
            if (unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)] != null && unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)].Item.TypeData.IsRange)
            {
                unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)].Reload();
            }
        }
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
        UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
        if (!(currentUnit == null))
        {
            if (targetData == null || targetData.unitCtrlr != currentUnit)
            {
                targetData = new TargetData(currentUnit);
            }
            if (unitCtrlr.Engaged || !unitCtrlr.HasRange())
            {
                unitCtrlr.unit.AddEnchantment(EnchantmentId.BASE_AMBUSH_OVERWATCH_REMOVER, unitCtrlr.unit, original: false);
                unitCtrlr.StateMachine.ChangeState(9);
            }
            else if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && unitCtrlr.GetWarband().teamIdx != currentUnit.GetWarband().teamIdx && !unitCtrlr.Engaged && !currentUnit.IsInFriendlyZone && (currentUnit.StateMachine.GetActiveStateId() == 11 || currentUnit.StateMachine.GetActiveStateId() == 42 || currentUnit.StateMachine.GetActiveStateId() == 43) && Vector3.SqrMagnitude(currentUnit.transform.position - currentUnit.startPosition) > Constant.GetFloat(ConstantId.MOVE_MINIMUM) * Constant.GetFloat(ConstantId.MOVE_MINIMUM) && unitCtrlr.CanTargetFromPoint(targetData, minDistance, distance, requiredPerc, unitBlocking: true, checkAllBones: true))
            {
                currentUnit.SendAskInterruption(UnitActionId.OVERWATCH, unitCtrlr.uid, currentUnit.transform.position, currentUnit.transform.rotation);
            }
        }
    }
}
