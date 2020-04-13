using UnityEngine;

public class NetControlled : ICheapState
{
    private UnitController unitCtrlr;

    public NetControlled(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogInfo("NetControlled Enter ", "UNIT_FLOW", unitCtrlr);
        unitCtrlr.SetFixed(fix: false);
        unitCtrlr.GetComponent<Rigidbody>().isKinematic = true;
        PandoraSingleton<MissionManager>.Instance.TurnOffActionZones();
    }

    void ICheapState.Exit(int iTo)
    {
        unitCtrlr.lastActionWounds = 0;
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_ACTION_CLEAR);
    }

    void ICheapState.Update()
    {
        unitCtrlr.UpdateTargetsData();
        PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
    }

    void ICheapState.FixedUpdate()
    {
    }
}
