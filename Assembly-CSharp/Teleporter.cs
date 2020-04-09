using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : TriggerPoint
{
	public GameObject exit;

	public List<GameObject> extraExits = new List<GameObject>();

	public GameObject prefabFx;

	private int currentExitIdx;

	public List<PointsChecker> PointsCheckers
	{
		get;
		private set;
	}

	private GameObject CurrentExit => (currentExitIdx != 0) ? extraExits[currentExitIdx - 1] : exit;

	private void Awake()
	{
		PointsCheckers = new List<PointsChecker>();
		PointsCheckers.Add(new PointsChecker(exit.transform, hasOffset: false));
		for (int i = 0; i < extraExits.Count; i++)
		{
			PointsCheckers.Add(new PointsChecker(extraExits[i].transform, hasOffset: false));
		}
	}

	private void Start()
	{
		PandoraSingleton<MissionManager>.Instance.triggerPoints.Add(this);
		Init();
	}

	public override void Trigger(UnitController currentUnit)
	{
		for (int i = 0; i < PointsCheckers.Count; i++)
		{
			if (PointsCheckers[i].IsAvailable())
			{
				currentExitIdx = i;
				break;
			}
		}
		if (prefabFx != null)
		{
			SpawnFx(base.transform);
		}
		base.Trigger(currentUnit);
	}

	private void SpawnFx(Transform parent)
	{
		if (prefabFx != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(prefabFx.name, parent, attached: true, null);
		}
	}

	public override bool IsActive()
	{
		for (int i = 0; i < PointsCheckers.Count; i++)
		{
			if (PointsCheckers[i].IsAvailable())
			{
				return true;
			}
		}
		return false;
	}

	public override void ActionOnUnit(UnitController currentUnit)
	{
		SpawnFx(CurrentExit.transform);
		currentUnit.InstantMove(CurrentExit.transform.position, CurrentExit.transform.rotation);
		PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(currentUnit, PointsCheckers[currentExitIdx], PointsCheckers[currentExitIdx].alliesOnZone, isEnemy: false);
		PandoraSingleton<MissionManager>.Instance.MoveUnitsOnActionZone(currentUnit, PointsCheckers[currentExitIdx], PointsCheckers[currentExitIdx].enemiesOnZone, isEnemy: true);
		currentUnit.SetFixed(fix: true);
	}

	private void OnDrawGizmos()
	{
		if (PointsCheckers == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < PointsCheckers.Count; i++)
		{
			for (int j = 0; j < PointsCheckers[i].validPoints.Count; j++)
			{
				Gizmos.DrawSphere(PointsCheckers[i].validPoints[j], 0.2f);
			}
		}
	}
}
