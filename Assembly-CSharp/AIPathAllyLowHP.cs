using RAIN.Core;
using System.Collections.Generic;

public class AIPathAllyLowHP : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathAllyLowHP";
	}

	protected override bool CheckAllies()
	{
		return true;
	}

	protected override void SetTargets(List<UnitController> allies)
	{
		for (int i = 0; i < allies.Count; i++)
		{
			if (allies[i] != unitCtrlr && (float)allies[i].unit.CurrentWound < (float)allies[i].unit.Wound * 0.35f && allies[i].Engaged)
			{
				targets.Add(allies[i]);
			}
		}
	}
}
