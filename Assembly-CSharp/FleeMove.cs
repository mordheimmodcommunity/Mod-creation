using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class FleeMove : ICheapState
{
	private const float BLOCK_TIME = 1f;

	private UnitController unitCtrlr;

	private bool pathDone;

	private GameObject pathRenderer;

	private FleePath path;

	private Vector2 flatPrevPos;

	private Vector2 flatNextPos;

	private Vector2 flatCurPos;

	private Vector3 previousNodePos;

	private Vector3 nextNodePos;

	private int waypointIdx;

	private Vector3 lastPosition;

	private float blockedTimer;

	private bool pathSearched;

	private bool moveDone;

	public FleeMove(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Update()
	{
		if (!PandoraSingleton<MissionManager>.Instance.IsNavmeshUpdating && !pathSearched)
		{
			pathSearched = true;
			FleePath fleePath = FleePath.Construct(unitCtrlr.transform.position, unitCtrlr.FleeTarget, (int)((float)unitCtrlr.unit.Movement * unitCtrlr.fleeDistanceMultiplier * 1000f));
			fleePath.aimStrength = 10f;
			int traversableTags = 1;
			PandoraSingleton<MissionManager>.Instance.PathSeeker.traversableTags = traversableTags;
			PandoraSingleton<MissionManager>.Instance.pathRayModifier.SetRadius(unitCtrlr.CapsuleRadius);
			PandoraSingleton<MissionManager>.Instance.PathSeeker.StartPath(fleePath, PathDone);
			if (unitCtrlr.IsPlayed())
			{
				PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitCtrlr.transform, transition: true, force: false, clearFocus: true, unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
			}
			else
			{
				PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.WATCH, null, transition: true, force: true);
				PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(unitCtrlr.transform);
			}
		}
		if (!pathDone || moveDone)
		{
			return;
		}
		if (unitCtrlr.IsPlayed())
		{
			PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(unitCtrlr);
		}
		else
		{
			PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(unitCtrlr);
		}
		flatPrevPos.x = previousNodePos.x;
		flatPrevPos.y = previousNodePos.z;
		flatNextPos.x = nextNodePos.x;
		flatNextPos.y = nextNodePos.z;
		ref Vector2 reference = ref flatCurPos;
		Vector3 position = unitCtrlr.transform.position;
		reference.x = position.x;
		ref Vector2 reference2 = ref flatCurPos;
		Vector3 position2 = unitCtrlr.transform.position;
		reference2.y = position2.z;
		if ((unitCtrlr.interactivePoints.Count == 0 && Vector2.SqrMagnitude(flatPrevPos - flatCurPos) > Vector2.SqrMagnitude(flatPrevPos - flatNextPos)) || (unitCtrlr.interactivePoints.Count != 0 && Vector2.SqrMagnitude(flatNextPos - flatCurPos) < 0.2f))
		{
			if (waypointIdx >= path.vectorPath.Count - 1)
			{
				EndMove();
				return;
			}
			waypointIdx++;
			previousNodePos = nextNodePos;
			nextNodePos = path.vectorPath[waypointIdx];
			int num = -1;
			for (int i = 0; i < path.path.Count; i++)
			{
				if (path.path[i].position == new Int3(nextNodePos))
				{
					num = i;
					break;
				}
			}
			if (num != -1 && unitCtrlr.interactivePoints.Count > 0 && num > 0 && path.path[num - 1].GraphIndex == 1 && path.path[num].GraphIndex == 1)
			{
				EndMove();
				return;
			}
		}
		if (Vector3.SqrMagnitude(unitCtrlr.transform.position - lastPosition) < 0.0100000007f)
		{
			blockedTimer += Time.deltaTime;
			if (blockedTimer >= 1f)
			{
				EndMove();
				return;
			}
		}
		else
		{
			blockedTimer = 0f;
		}
		unitCtrlr.SetAnimSpeed(1f);
		Quaternion quaternion = default(Quaternion);
		quaternion.SetLookRotation(nextNodePos - unitCtrlr.transform.position, Vector3.up);
		Vector3 eulerAngles = quaternion.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		unitCtrlr.transform.rotation = Quaternion.Euler(eulerAngles);
		lastPosition = unitCtrlr.transform.position;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		unitCtrlr.SetFleeTarget();
		PandoraSingleton<MissionManager>.Instance.TurnTimer.Pause();
		pathDone = false;
		previousNodePos = unitCtrlr.transform.position;
		nextNodePos = unitCtrlr.transform.position;
		lastPosition = Vector3.zero;
		waypointIdx = 0;
		blockedTimer = 0f;
		List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
		for (int i = 0; i < allUnits.Count; i++)
		{
			if (allUnits[i].unit.Status != UnitStateId.OUT_OF_ACTION && !allUnits[i].isNavCutterActive())
			{
				allUnits[i].SetGraphWalkability(walkable: false);
			}
		}
		unitCtrlr.SetGraphWalkability(walkable: true);
		PandoraDebug.LogInfo("Flee move distance " + (int)((float)unitCtrlr.unit.Movement * unitCtrlr.fleeDistanceMultiplier * 1000f));
		pathSearched = false;
		moveDone = true;
		if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() != unitCtrlr && PandoraSingleton<Hermes>.Instance.IsConnected())
		{
			unitCtrlr.StartSync();
		}
	}

	public void Exit(int iTo)
	{
		unitCtrlr.fleeDistanceMultiplier = Constant.GetFloat(ConstantId.FLEE_MOVEMENT_MULTIPLIER);
		unitCtrlr.SetGraphWalkability(walkable: false);
	}

	public void FixedUpdate()
	{
	}

	private void PathDone(Path p)
	{
		path = (FleePath)p;
		path.Claim(unitCtrlr);
		if (p.vectorPath.Count > 0)
		{
			pathDone = true;
			moveDone = false;
			nextNodePos = path.vectorPath[waypointIdx];
			unitCtrlr.SetFixed(fix: false);
			unitCtrlr.SetKinemantic(kine: false);
		}
		else
		{
			EndMove();
		}
	}

	private void EndMove()
	{
		moveDone = true;
		path.Release(unitCtrlr);
		unitCtrlr.SetAnimSpeed(0f);
		unitCtrlr.SetFixed(fix: true);
		unitCtrlr.SetKinemantic(kine: true);
		if (PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() != unitCtrlr && PandoraSingleton<Hermes>.Instance.IsConnected())
		{
			unitCtrlr.StopSync();
		}
		unitCtrlr.SendStartMove(unitCtrlr.transform.position, unitCtrlr.transform.rotation);
	}
}
