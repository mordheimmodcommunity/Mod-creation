using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIMoveOnPathRange : AIMoveOnPath
{
    private int minRange;

    private ActionStatus action;

    private List<UnitController> targets;

    private UnitController target;

    private bool shooting;

    private int rangeMin;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "MoveOnPathRange";
        minRange = 0;
        if (unitCtrlr.HasRange())
        {
            minRange = unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].Item.RangeMin;
        }
        shooting = false;
        target = null;
        action = unitCtrlr.GetAction(SkillId.BASE_SHOOT);
        targets = action.Targets;
        rangeMin = action.RangeMin;
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_0136: Unknown result type (might be due to invalid IL or missing references)
        action.UpdateAvailable();
        if (action.Available)
        {
            if (unitCtrlr.AICtrlr.targetEnemy != null)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == unitCtrlr.AICtrlr.targetEnemy)
                    {
                        SetShoot(targets[i]);
                        return (ActionResult)0;
                    }
                }
            }
            else if (targets.Count > 0)
            {
                SetShoot(targets[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, targets.Count)]);
                return (ActionResult)0;
            }
        }
        if (unitCtrlr.AICtrlr.targetEnemy != null && Vector3.SqrMagnitude(unitCtrlr.transform.position - unitCtrlr.AICtrlr.targetEnemy.transform.position) < (float)(minRange * minRange))
        {
            return (ActionResult)0;
        }
        return base.Execute(ai);
    }

    public override void Stop(AI ai)
    {
        unitCtrlr.AICtrlr.targetEnemy = null;
        base.Stop(ai);
    }

    public override void GotoNextState()
    {
        if (shooting && target != null)
        {
            unitCtrlr.SendSkillSingleTarget(SkillId.BASE_SHOOT, target);
        }
        else
        {
            base.GotoNextState();
        }
    }

    private void SetShoot(UnitController unit)
    {
        target = unit;
        shooting = true;
        EndMove();
    }
}
