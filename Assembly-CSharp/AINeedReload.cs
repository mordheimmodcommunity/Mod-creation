using RAIN.Core;

public class AINeedReload : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "NeedReload";
        success = unitCtrlr.GetAction(SkillId.BASE_RELOAD).Available;
    }
}
