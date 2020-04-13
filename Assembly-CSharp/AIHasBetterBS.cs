using RAIN.Core;

public class AIHasBetterBS : AIBase
{
    private const float WS_RATIO = 1.25f;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIHasBetterBS";
        success = ((float)unitCtrlr.unit.BallisticSkill >= (float)unitCtrlr.unit.WeaponSkill * 1.25f);
    }
}
