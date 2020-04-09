using RAIN.Core;
using System.Collections.Generic;

public class AIPathWyrdStoneClosest : AIPathSearchBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathWyrdStoneClosest";
	}

	public override List<SearchPoint> GetTargets()
	{
		return PandoraSingleton<MissionManager>.Instance.GetWyrdstonePoints();
	}
}
