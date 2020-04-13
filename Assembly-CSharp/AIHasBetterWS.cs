using RAIN.Core;

public class AIHasBetterWS : AIBase
{
    private const float BS_RATIO = 0.8f;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIHasBetterWS";
        success = ((float)unitCtrlr.unit.WeaponSkill >= (float)unitCtrlr.unit.BallisticSkill * 0.8f);
    }
}
