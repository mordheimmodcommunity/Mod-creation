using RAIN.Core;

public class AIIsPlayObjectiveBounty : AIBase
{
    private WarbandController warCtrlr;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "IsPlayObjectiveBounty";
        warCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
        success = (warCtrlr.objectives[0].TypeId == PrimaryObjectiveTypeId.BOUNTY);
    }
}
