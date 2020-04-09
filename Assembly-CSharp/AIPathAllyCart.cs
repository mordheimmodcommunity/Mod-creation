using RAIN.Core;
using System.Collections.Generic;

public class AIPathAllyCart : AIPathSearchBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathAllyCart";
	}

	public override List<SearchPoint> GetTargets()
	{
		List<SearchPoint> list = new List<SearchPoint>();
		list.Add(unitCtrlr.GetWarband().wagon.chest);
		return list;
	}
}
