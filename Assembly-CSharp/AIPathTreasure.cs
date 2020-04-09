using RAIN.Core;
using System.Collections.Generic;

public class AIPathTreasure : AIPathSearchBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathTreasure";
	}

	public override List<SearchPoint> GetTargets()
	{
		int currentStrategyPoints = unitCtrlr.unit.CurrentStrategyPoints;
		currentStrategyPoints -= unitCtrlr.GetAction(SkillId.BASE_SEARCH).StrategyPoints;
		if (currentStrategyPoints > 0 && !unitCtrlr.unit.IsInventoryFull())
		{
			float dist = currentStrategyPoints * unitCtrlr.unit.Movement;
			return PandoraSingleton<MissionManager>.Instance.GetSearchPointInRadius(unitCtrlr.transform.position, dist, UnitActionId.SEARCH);
		}
		return new List<SearchPoint>();
	}
}
