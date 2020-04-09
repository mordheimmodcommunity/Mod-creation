using RAIN.Core;
using System.Collections.Generic;

public class AIPathImpressiveEnemy : AIPathUnitBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PathImpressiveEnemy";
	}

	protected override bool CheckAllies()
	{
		return false;
	}

	protected override void SetTargets(List<UnitController> enemies)
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].unit.GetUnitTypeId() == UnitTypeId.IMPRESSIVE)
			{
				targets.Add(enemies[i]);
			}
		}
	}
}
