using System.Collections.Generic;
using UnityEngine;

public class LineTargeting : ICheapState
{
	private const float MIN_DIST = 0.01f;

	private UnitController unitCtrlr;

	private CameraManager cam;

	private Vector3 lineSrc;

	private Vector3 lineDir;

	private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

	public LineTargeting(UnitController ctrlr)
	{
		unitCtrlr = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		unitCtrlr.SetFixed(fix: true);
		cam = PandoraSingleton<MissionManager>.Instance.CamManager;
		cam.SwitchToCam(CameraManager.CameraType.FIXED, unitCtrlr.transform);
		unitCtrlr.defenders.Clear();
		unitCtrlr.destructTargets.Clear();
		availableTargets = unitCtrlr.GetCurrentActionTargets();
		PandoraSingleton<MissionManager>.Instance.InitLineTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.RangeMax, out lineSrc, out lineDir);
		SetLinePos();
		PandoraSingleton<MissionManager>.Instance.lineTarget.SetActive(value: true);
		if (unitCtrlr.IsPlayed())
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
		}
	}

	void ICheapState.Exit(int iTo)
	{
		unitCtrlr.ClearFlyingTexts();
		PandoraSingleton<MissionManager>.Instance.lineTarget.SetActive(value: false);
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
			unitCtrlr.SendSkillTargets(unitCtrlr.CurrentAction.SkillId, lineSrc, lineDir);
			return;
		}
		float num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
		float num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
		num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
		num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
		if (num != 0f || num2 != 0f)
		{
			Quaternion lhs = Quaternion.AngleAxis(num, Vector3.up);
			Quaternion rhs = Quaternion.AngleAxis(num2, unitCtrlr.transform.right);
			Vector3 to = lhs * rhs * lineDir;
			if (Vector3.Angle(unitCtrlr.transform.forward, to) < 90f)
			{
				lineDir = to;
			}
		}
		SetLinePos();
		unitCtrlr.SetLineTargets(availableTargets, PandoraSingleton<MissionManager>.Instance.lineTarget.transform, highlighTargets: true);
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void SetLinePos()
	{
		PandoraSingleton<MissionManager>.Instance.lineTarget.transform.position = lineSrc;
		Quaternion rotation = Quaternion.LookRotation(lineDir);
		PandoraSingleton<MissionManager>.Instance.lineTarget.transform.rotation = rotation;
		unitCtrlr.FaceTarget(PandoraSingleton<MissionManager>.Instance.lineTarget.transform.GetChild(0));
		cam.SetShoulderCam(unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
		cam.dummyCam.transform.rotation = rotation;
	}
}
