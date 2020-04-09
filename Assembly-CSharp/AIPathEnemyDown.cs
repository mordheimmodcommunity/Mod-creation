using RAIN.Core;
using System.Collections.Generic;

public class AIPathEnemyDown : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.actionName = "PathEnemyDown";
		base.Start(ai);
	}

	protected override bool CheckAllies()
	{
		return false;
	}

	protected override void SetTargets(List<UnitController> enemies)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].unit.Status == UnitStateId.KNOCKED_DOWN || enemies[i].unit.Status == UnitStateId.STUNNED)
			{
				targets.Add(enemies[i]);
			}
		}
	}
}
