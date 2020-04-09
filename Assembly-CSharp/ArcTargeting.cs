using System.Collections.Generic;
using UnityEngine;

public class ArcTargeting : ICheapState
{
	private const float MIN_DIST = 0.01f;

	private UnitController unitCtrlr;

	private CameraManager cam;

	private Vector3 arcSrc;

	private Vector3 arcDir;

	private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

	public ArcTargeting(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		cam = PandoraSingleton<MissionManager>.Instance.CamManager;
		unitCtrlr.SetFixed(fix: true);
		cam.SwitchToCam(CameraManager.CameraType.FIXED, unitCtrlr.transform);
		unitCtrlr.defenders.Clear();
		unitCtrlr.destructTargets.Clear();
		availableTargets = unitCtrlr.GetCurrentActionTargets();
		PandoraSingleton<MissionManager>.Instance.InitArcTarget(unitCtrlr.transform, out arcSrc, out arcDir);
		SetArcPos();
		PandoraSingleton<MissionManager>.Instance.arcTarget.SetActive(value: true);
		if (unitCtrlr.IsPlayed())
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
		}
	}

	void ICheapState.Exit(int iTo)
	{
		unitCtrlr.ClearFlyingTexts();
		PandoraSingleton<MissionManager>.Instance.arcTarget.SetActive(value: false);
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CANCEL);
			unitCtrlr.StateMachine.ChangeState((!unitCtrlr.Engaged) ? 11 : 12);
			return;
		}
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_ACTION_CONFIRM);
			unitCtrlr.SendSkillTargets(unitCtrlr.CurrentAction.SkillId, PandoraSingleton<MissionManager>.Instance.arcTarget.transform.position, PandoraSingleton<MissionManager>.Instance.arcTarget.transform.forward);
			return;
		}
		float num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
		num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
		if (num != 0f)
		{
			Quaternion rotation = Quaternion.AngleAxis(num, Vector3.up);
			Vector3 to = rotation * arcDir;
			if (Vector3.Angle(unitCtrlr.transform.forward, to) < 90f)
			{
				arcDir = to;
			}
		}
		SetArcPos();
		unitCtrlr.SetArcTargets(availableTargets, unitCtrlr.transform.forward, highlightTargets: true);
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void SetArcPos()
	{
		PandoraSingleton<MissionManager>.Instance.arcTarget.transform.position = arcSrc;
		Quaternion rotation = Quaternion.LookRotation(arcDir);
		PandoraSingleton<MissionManager>.Instance.arcTarget.transform.rotation = rotation;
		unitCtrlr.FaceTarget(PandoraSingleton<MissionManager>.Instance.arcTarget.transform.GetChild(0));
		cam.SetShoulderCam(unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
		cam.dummyCam.transform.rotation = rotation;
		cam.dummyCam.transform.position += cam.dummyCam.transform.forward * -1.5f + cam.dummyCam.transform.right * -0.25f;
	}
}
