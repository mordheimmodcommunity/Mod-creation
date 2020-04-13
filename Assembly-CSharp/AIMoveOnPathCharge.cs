using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public class AIMoveOnPathCharge : AIMoveOnPath
{
    private UnitController refTarget;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "MoveOnPathCharge";
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_00a7: Unknown result type (might be due to invalid IL or missing references)
        ActionStatus action = unitCtrlr.GetAction(SkillId.BASE_CHARGE);
        action.UpdateAvailable();
        if (action.Available && unitCtrlr.chargeTargets.IndexOf(unitCtrlr.AICtrlr.targetEnemy) != -1)
        {
            SkillId skillId = unitCtrlr.AICtrlr.GetBestAction(AIController.chargeActions, out refTarget, RefineTargets)?.SkillId ?? SkillId.BASE_CHARGE;
            unitCtrlr.SendSkillSingleTarget(skillId, unitCtrlr.AICtrlr.targetEnemy);
            return (ActionResult)0;
        }
        return base.Execute(ai);
    }

    public override void Stop(AI ai)
    {
        unitCtrlr.AICtrlr.targetEnemy = null;
        base.Stop(ai);
    }

    private void RefineTargets(ActionStatus action, List<UnitController> allTargets)
    {
        allTargets.Clear();
        allTargets.Add(unitCtrlr.AICtrlr.targetEnemy);
    }
}
