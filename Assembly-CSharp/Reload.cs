public class Reload : ICheapState
{
    private UnitController unitCtrlr;

    public Reload(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
        PandoraSingleton<MissionManager>.Instance.PlaySequence("reload", unitCtrlr, OnSeqDone);
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    public void ReloadWeapon(int slot)
    {
        if (unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + slot)] != null && unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + slot)].Item.TypeData.IsRange)
        {
            unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + slot)].Reload();
        }
    }

    private void OnSeqDone()
    {
        unitCtrlr.nextState = UnitController.State.START_MOVE;
    }
}
