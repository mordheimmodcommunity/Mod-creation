using RAIN.Core;

public class AIHasTargetsInRange : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIHasTargetsInRange";
        ActionStatus action = unitCtrlr.GetAction(SkillId.BASE_SHOOT);
        action.SetTargets();
        success = (action.Targets.Count > 0);
    }
}
