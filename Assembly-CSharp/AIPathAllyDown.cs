using RAIN.Core;
using System.Collections.Generic;

public class AIPathAllyDown : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathAllyDown";
	}

	protected override bool CheckAllies()
	{
		return true;
	}

	protected override void SetTargets(List<UnitController> allies)
	{
		for (int i = 0; i < allies.Count; i++)
		{
			if (allies[i] != unitCtrlr && (allies[i].unit.Status == UnitStateId.KNOCKED_DOWN || allies[i].unit.Status == UnitStateId.STUNNED) && allies[i].Engaged)
			{
				targets.Add(allies[i]);
			}
		}
	}
}
