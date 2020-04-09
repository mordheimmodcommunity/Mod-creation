using RAIN.Core;
using RAIN.Memory;
using RAIN.Representation;
using UnityEngine;

public class AIHasLowHealth : AIBase
{
	public Expression expr;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "HasLowHealth";
		int num = expr.Evaluate<int>(Time.deltaTime, (RAINMemory)null);
		success = ((float)unitCtrlr.unit.CurrentWound <= (float)(unitCtrlr.unit.Wound * num) / 100f);
	}
}
