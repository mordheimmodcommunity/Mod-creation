using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyAllyDown : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathEnemyAllyDown";
	}

	protected override bool CheckAllies()
	{
		return true;
	}

	protected override void SetTargets(List<UnitController> allies)
	{
		for (int i = 0; i < allies.Count; i++)
		{
			if (allies[i] != unitCtrlr && allies[i].unit.Status == UnitStateId.STUNNED && allies[i].Engaged)
			{
				targets.AddRange(allies[i].EngagedUnits);
			}
		}
	}
}
