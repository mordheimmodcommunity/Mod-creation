using System.Collections.Generic;

public class PrepareFly : ICheapState
{
	private UnitController unitCtrlr;

	private FlyPoint bestPoint;

	public PrepareFly(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		if (unitCtrlr.AICtrlr == null)
		{
			List<DecisionPoint> decisionPoints = PandoraSingleton<MissionManager>.Instance.GetDecisionPoints(unitCtrlr, DecisionPointId.FLY, float.MaxValue, excludeCloseToUnits: false);
			bestPoint = (FlyPoint)decisionPoints[0];
		}
		else
		{
			bestPoint = (FlyPoint)unitCtrlr.AICtrlr.targetDecisionPoint;
		}
		bestPoint.PointsChecker.UpdateControlPoints(unitCtrlr, PandoraSingleton<MissionManager>.Instance.GetAllAliveUnits());
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(unitCtrlr, bestPoint.PointsChecker, bestPoint.PointsChecker.alliesOnZone, isEnemy: false);
			PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(unitCtrlr, bestPoint.PointsChecker, bestPoint.PointsChecker.enemiesOnZone, isEnemy: true);
		}
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.SetKinemantic(kine: true);
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer())
		{
			unitCtrlr.SendFly();
		}
	}

	void ICheapState.FixedUpdate()
	{
	}
}
