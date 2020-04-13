using System.Collections.Generic;
using UnityEngine;

public class Flee : ICheapState
{
    private UnitController unitCtrlr;

    private Stack<UnitController> freeAttackers;

    private bool checkNext;

    public Flee(UnitController ctrler)
    {
        unitCtrlr = ctrler;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogDebug("Flee Enter", "FLOW", unitCtrlr);
        unitCtrlr.SetFixed(fix: false);
        unitCtrlr.GetComponent<Rigidbody>().isKinematic = true;
        if (freeAttackers == null)
        {
            freeAttackers = new Stack<UnitController>();
            for (int i = 0; i < unitCtrlr.EngagedUnits.Count; i++)
            {
                if (unitCtrlr.EngagedUnits[i].unit.IsAvailable() && unitCtrlr.EngagedUnits[i].HasClose())
                {
                    freeAttackers.Push(unitCtrlr.EngagedUnits[i]);
                }
            }
            PandoraDebug.LogDebug("Flee Enter FreeAttackers size = " + freeAttackers.Count);
        }
        unitCtrlr.Fleeing = true;
        checkNext = true;
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
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
        PandoraDebug.LogDebug("Flee NextFreeAttack");
        if (freeAttackers.Count > 0 && unitCtrlr.unit.Status != UnitStateId.OUT_OF_ACTION)
        {
            UnitController unitController = freeAttackers.Pop();
            unitController.defenderCtrlr = unitCtrlr;
            PandoraDebug.LogDebug("Flee NextFreeAttack Pop!");
            if (unitController.unit.IsAvailable())
            {
                unitController.FaceTarget(unitController.defenderCtrlr.transform);
                unitCtrlr.WaitForAction(UnitController.State.FLEE);
                PandoraDebug.LogDebug("Flee NextFreeAttack Launch Attack");
                unitController.SkillSingleTargetRPC(386, unitCtrlr.uid);
            }
            else
            {
                checkNext = true;
            }
            return;
        }
        PandoraDebug.LogDebug("Flee NextFreeAttack No free attacker");
        freeAttackers = null;
        unitCtrlr.Fleeing = false;
        if (unitCtrlr.unit.IsAvailable())
        {
            PandoraDebug.LogDebug("Flee NextFreeAttack Switch to Disengage");
            unitCtrlr.SetCurrentAction(SkillId.BASE_FLEE);
            if (unitCtrlr.IsMine())
            {
                unitCtrlr.StateMachine.ChangeState(41);
            }
            else
            {
                unitCtrlr.StateMachine.ChangeState(43);
            }
        }
        else
        {
            unitCtrlr.StateMachine.ChangeState(10);
        }
    }
}
