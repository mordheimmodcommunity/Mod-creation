using System.Collections;
using UnityEngine;

public class Teleport : ICheapState
{
    private UnitController unitCtrlr;

    private Teleporter teleport;

    public Teleport(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        teleport = (Teleporter)unitCtrlr.activeTrigger;
        unitCtrlr.currentTeleporter = null;
        unitCtrlr.SetAnimSpeed(0f);
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.SetKinemantic(kine: true);
        if (unitCtrlr == PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
        {
            unitCtrlr.ValidMove();
        }
        unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_teleport"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/teleport", cached: true));
        teleport.Trigger(unitCtrlr);
        unitCtrlr.Hide(hide: true);
        unitCtrlr.StartCoroutine(WaitForDissolved(1f));
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

    private IEnumerator WaitForDissolved(float time)
    {
        yield return new WaitForSeconds(time);
        teleport.ActionOnUnit(unitCtrlr);
        if (unitCtrlr.IsPlayed())
        {
            unitCtrlr.Hide(hide: false, force: false, OnSeqDone);
            yield break;
        }
        PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
        if (unitCtrlr.Imprint.State == MapImprintStateId.VISIBLE)
        {
            unitCtrlr.Hide(hide: false);
            unitCtrlr.StartCoroutine(WaitForSeqDone(1f));
        }
        else
        {
            OnSeqDone();
        }
    }

    private IEnumerator WaitForSeqDone(float time)
    {
        yield return new WaitForSeconds(time);
        OnSeqDone();
    }

    public void OnSeqDone()
    {
        unitCtrlr.activeTrigger = null;
        unitCtrlr.StateMachine.ChangeState(10);
    }
}
