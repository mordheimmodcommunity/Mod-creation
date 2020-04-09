using RAIN.Core;

public class AIIsPlayObjectiveRush : AIBase
{
	private WarbandController warCtrlr;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "IsPlayObjectiveRush";
		warCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
		success = (warCtrlr.objectives[0].TypeId == PrimaryObjectiveTypeId.WYRDSTONE_RUSH);
	}
}
