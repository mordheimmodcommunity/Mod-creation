using RAIN.Core;

public class AIIsPlayObjectiveGTC : AIBase
{
	private WarbandController warCtrlr;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "IsPlayObjectiveGTC";
		warCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
		success = (warCtrlr.objectives[0].Id == PrimaryObjectiveId.GRAND_THEFT_CART);
	}
}
