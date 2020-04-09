using System.Collections.Generic;
using UnityEngine;

public class WatchCamera : ICheapState
{
	private const float TIME_TO_SWITCH = 0.5f;

	public Vector3 targetPosition = Vector3.zero;

	private UnitController watcher;

	private UnitController previousEnemy;

	public UnitController lastWatcher;

	private float prevDistance;

	private float watcherDistance;

	private float visionTime;

	private CameraManager mngr;

	private Transform camTrans;

	private Transform dummyCam;

	public WatchCamera(CameraManager camMngr)
	{
		mngr = camMngr;
		camTrans = mngr.transform;
		dummyCam = mngr.dummyCam.transform;
	}

	public void Destroy()
	{
	}

	public void Enter(int from)
	{
		watcher = null;
		previousEnemy = null;
		lastWatcher = null;
		prevDistance = 10000f;
		watcherDistance = 10000f;
	}

	public void Exit(int to)
	{
	}

	public void Update()
	{
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (currentUnit == null || !currentUnit.IsImprintVisible())
		{
			visionTime = 5f;
			return;
		}
		if (currentUnit != previousEnemy)
		{
			visionTime = 5f;
		}
		watcher = null;
		watcherDistance = 10000f;
		bool flag = false;
		List<UnitController> viewers = currentUnit.Imprint.Viewers;
		for (int i = 0; i < viewers.Count; i++)
		{
			if (viewers[i].unit.Status != UnitStateId.OUT_OF_ACTION)
			{
				float num = Vector3.SqrMagnitude(viewers[i].transform.position - currentUnit.transform.position);
				if (viewers[i] == lastWatcher)
				{
					flag = true;
					prevDistance = num;
				}
				if (num < watcherDistance)
				{
					watcher = viewers[i];
					watcherDistance = num;
				}
			}
		}
		UnitController unitController = null;
		visionTime += Time.deltaTime;
		if (!flag && watcher != null && visionTime > 0.5f)
		{
			visionTime = 0f;
			unitController = watcher;
		}
		else if (flag)
		{
			if (lastWatcher != watcher && watcher != null)
			{
				if (watcherDistance > prevDistance / 2f)
				{
					visionTime = 0f;
				}
				unitController = ((!(watcher != null) || !(visionTime > 0.5f)) ? lastWatcher : watcher);
			}
			else
			{
				visionTime = 0f;
				unitController = lastWatcher;
			}
		}
		if (!unitController)
		{
			return;
		}
		Transform transform = unitController.transform;
		float num2 = 1.5f;
		if (currentUnit.unit.Data.UnitSizeId == UnitSizeId.LARGE)
		{
			num2 = 2f;
		}
		mngr.SetZoomDiff(unitController.unit.Data.UnitSizeId == UnitSizeId.LARGE);
		mngr.SetTarget(unitController.transform);
		mngr.LookAtFocus(currentUnit.transform, overrideCurrentTarget: false, transition: false);
		mngr.SetShoulderCam(unitController.unit.Data.UnitSizeId == UnitSizeId.LARGE);
		if (unitController != lastWatcher)
		{
			mngr.Transition(2f, force: false);
		}
		lastWatcher = unitController;
		dummyCam.LookAt(currentUnit.transform.position + Vector3.up * num2);
		mngr.SetDOFTarget(currentUnit.transform, num2);
		if (previousEnemy != currentUnit)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_SHOW_ENEMY, v1: true);
			if ((bool)unitController && (bool)currentUnit)
			{
				PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.UPDATE_TARGET, currentUnit, currentUnit.unit.warbandIdx);
			}
			previousEnemy = currentUnit;
		}
	}

	public void FixedUpdate()
	{
	}
}
