using RAIN.Action;
using RAIN.Core;

public class AISearch : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "Search";
		success = (unitCtrlr.AICtrlr.targetSearchPoint != null && !unitCtrlr.AICtrlr.AlreadyLootSearchPoint(unitCtrlr.AICtrlr.targetSearchPoint) && unitCtrlr.interactivePoints.IndexOf(unitCtrlr.AICtrlr.targetSearchPoint) != -1 && unitCtrlr.GetAction(SkillId.BASE_SEARCH).Available);
		if (!success)
		{
			return;
		}
		success = false;
		int num = 0;
		while (true)
		{
			if (num < unitCtrlr.interactivePoints.Count)
			{
				if (unitCtrlr.interactivePoints[num] == unitCtrlr.AICtrlr.targetSearchPoint)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		success = true;
	}

	public override ActionResult Execute(AI ai)
	{
		if (success)
		{
			unitCtrlr.AICtrlr.lootedSearchPoints.Add(unitCtrlr.AICtrlr.targetSearchPoint);
			unitCtrlr.SendInteractiveAction(SkillId.BASE_SEARCH, unitCtrlr.AICtrlr.targetSearchPoint);
			return (ActionResult)0;
		}
		return (ActionResult)2;
	}
}
