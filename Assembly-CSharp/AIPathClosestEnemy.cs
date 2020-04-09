using RAIN.Core;
using System.Collections.Generic;

public class AIPathClosestEnemy : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathClosestEnemy";
	}

	protected override bool CheckAllies()
	{
		return false;
	}

	protected override void SetTargets(List<UnitController> all)
	{
		targets.AddRange(all);
	}
}
