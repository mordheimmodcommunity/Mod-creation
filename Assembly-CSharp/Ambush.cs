using UnityEngine;

public class Ambush : ICheapState
{
    private UnitController unitCtrlr;

    public Ambush(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
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
        if (unitCtrlr.Engaged || !unitCtrlr.HasClose())
        {
            unitCtrlr.unit.AddEnchantment(EnchantmentId.BASE_AMBUSH_OVERWATCH_REMOVER, unitCtrlr.unit, original: false);
            unitCtrlr.StateMachine.ChangeState(9);
        }
        else if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && currentUnit != null && unitCtrlr.GetWarband().teamIdx != currentUnit.GetWarband().teamIdx && !currentUnit.IsInFriendlyZone && (currentUnit.StateMachine.GetActiveStateId() == 11 || currentUnit.StateMachine.GetActiveStateId() == 42 || currentUnit.StateMachine.GetActiveStateId() == 43) && Vector3.SqrMagnitude(currentUnit.transform.position - currentUnit.startPosition) > Constant.GetFloat(ConstantId.MOVE_MINIMUM) * Constant.GetFloat(ConstantId.MOVE_MINIMUM) && unitCtrlr.CanChargeUnit(currentUnit, isAmbush: true))
        {
            currentUnit.SendAskInterruption(UnitActionId.AMBUSH, unitCtrlr.uid, currentUnit.transform.position, currentUnit.transform.rotation);
        }
    }
}
