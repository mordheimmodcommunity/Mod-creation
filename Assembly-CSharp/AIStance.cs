using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public class AIStance : AIBase
{
    private ActionStatus bestAction;

    private UnitController refTarget;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "Stance";
        bestAction = unitCtrlr.AICtrlr.GetBestAction(AIController.stanceActions, out refTarget, delegate (ActionStatus action, List<UnitController> list)
        {
            list.Clear();
            list.Add(unitCtrlr);
        });
        success = (bestAction != null);
    }

    public override ActionResult Execute(AI ai)
    {
        if (success)
        {
            unitCtrlr.SendSkill(bestAction.SkillId);
            return (ActionResult)0;
        }
        return (ActionResult)2;
    }
}
