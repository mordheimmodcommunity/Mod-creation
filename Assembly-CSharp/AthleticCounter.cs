using System.Collections.Generic;
using UnityEngine;

public class AthleticCounter : ICheapState
{
    private UnitController unitCtrlr;

    private Stack<UnitController> freeAttackers;

    private bool checkNext;

    public AthleticCounter(UnitController ctrler)
    {
        unitCtrlr = ctrler;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogDebug("AthleticCounter Enter", "FLOW", unitCtrlr);
        unitCtrlr.SetFixed(fix: false);
        unitCtrlr.GetComponent<Rigidbody>().isKinematic = true;
        if (freeAttackers == null)
        {
            freeAttackers = new Stack<UnitController>();
            for (int i = 0; i < unitCtrlr.EngagedUnits.Count; i++)
            {
                if (unitCtrlr.EngagedUnits[i].CanCounterAttack())
                {
                    freeAttackers.Push(unitCtrlr.EngagedUnits[i]);
                }
            }
            PandoraDebug.LogDebug("AthleticCounter Enter Attackers size = " + freeAttackers.Count);
        }
        unitCtrlr.Fleeing = true;
        checkNext = true;
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
        unitCtrlr.UpdateTargetsData();
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(unitCtrlr);
        }
        else
        {
            PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
        }
        if (checkNext)
        {
            checkNext = false;
            NextFreeAttack();
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    public void NextFreeAttack()
    {
        PandoraDebug.LogDebug("AthleticCounter NextFreeAttack");
        if (freeAttackers.Count > 0 && unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            PandoraDebug.LogDebug("AthleticCounter NextAttack Pop!");
            UnitController unitController = freeAttackers.Pop();
            unitController.attackerCtrlr = unitCtrlr;
            unitController.UpdateActionStatus(notice: false);
            if (unitController.unit.IsAvailable())
            {
                unitController.FaceTarget(unitCtrlr.transform);
                unitCtrlr.WaitForAction(UnitController.State.ATHLETIC_COUNTER);
                PandoraDebug.LogDebug("AthleticCounter NextAttack Launch Attack");
                unitController.StateMachine.ChangeState(34);
            }
            else
            {
                checkNext = true;
            }
        }
        else
        {
            PandoraDebug.LogDebug("AthleticCounter NextAttack No more attacker");
            freeAttackers = null;
            unitCtrlr.Fleeing = false;
            unitCtrlr.StateMachine.ChangeState(10);
        }
    }
}
