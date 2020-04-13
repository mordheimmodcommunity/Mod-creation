using System.Collections;
using UnityEngine;

public class ActivateStance : ICheapState
{
    private UnitController unitCtrlr;

    public ActivateStance(UnitController ctrlr)
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
        PandoraSingleton<MissionManager>.Instance.PlaySequence("skill", unitCtrlr, OnSeqDone);
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

    private IEnumerator ShowOutcome()
    {
        yield return new WaitForSeconds(0.5f);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, unitCtrlr);
    }

    private void OnSeqDone()
    {
        unitCtrlr.SkillRPC(339);
    }
}
