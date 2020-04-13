using System.Collections;
using UnityEngine;

public class Activate : ICheapState
{
    private UnitController unitCtrlr;

    public Activate(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        if ((Object)(object)unitCtrlr.interactivePoint.Highlight != null)
        {
            unitCtrlr.FaceTarget(((Component)(object)unitCtrlr.interactivePoint.Highlight).transform, force: true);
        }
        else
        {
            unitCtrlr.FaceTarget(unitCtrlr.interactivePoint.transform, force: true);
        }
        PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitCtrlr.interactivePoint.transform.parent);
        unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
        unitCtrlr.searchVariation = (int)unitCtrlr.interactivePoint.anim;
        unitCtrlr.GetWarband().Activate(unitCtrlr.interactivePoint.name);
        PandoraSingleton<MissionManager>.Instance.PlaySequence("search_start", unitCtrlr, OnSeqDone);
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

    public void ActivatePoint()
    {
        ((ActivatePoint)unitCtrlr.interactivePoint).Activate(unitCtrlr);
        unitCtrlr.LaunchAction(UnitActionId.ACTIVATE, success: true, unitCtrlr.unit.Status, 2);
    }

    private void OnSeqDone()
    {
        unitCtrlr.StartCoroutine(WaitForAnims());
    }

    private IEnumerator WaitForAnims()
    {
        while (((ActivatePoint)unitCtrlr.interactivePoint).IsAnimationPlaying())
        {
            yield return null;
        }
        PandoraSingleton<MissionManager>.Instance.RefreshGraph();
        ActivatePoint point = (ActivatePoint)unitCtrlr.interactivePoint;
        if (point.consumeRequestedItem)
        {
            unitCtrlr.unit.ConsumeItem(point.requestedItemId);
        }
        point.SpawnCampaignUnit();
        point.ActivateZoneAoe();
        if (!PandoraSingleton<MissionManager>.Instance.CheckEndGame())
        {
            if (point.curseId != 0)
            {
                unitCtrlr.currentSpellTypeId = SpellTypeId.MISSION;
                unitCtrlr.currentCurseSkillId = point.curseId;
                unitCtrlr.StateMachine.ChangeState(29);
            }
            else
            {
                unitCtrlr.StateMachine.ChangeState(10);
            }
        }
    }
}
