using UnityEngine;

public class Idle : ICheapState
{
    private UnitController unitCtrlr;

    public Idle(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogInfo("Idle Enter ", "UNIT_FLOW", unitCtrlr);
        unitCtrlr.SetFixed(fix: true);
        if (unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            unitCtrlr.GetComponent<Collider>().enabled = true;
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
    }
}
