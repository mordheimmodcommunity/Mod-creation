using System.Collections.Generic;
using UnityEngine;

public class ConeTargeting : ICheapState
{
    private const float MIN_DIST = 0.01f;

    private UnitController unitCtrlr;

    private CameraManager cam;

    private Vector3 coneSrc;

    private Vector3 coneDir;

    private List<MonoBehaviour> availableTargets = new List<MonoBehaviour>();

    public ConeTargeting(UnitController ctrlr)
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
        PandoraSingleton<MissionManager>.Instance.InitConeTarget(unitCtrlr.transform, unitCtrlr.CurrentAction.Radius, unitCtrlr.CurrentAction.RangeMax, out coneSrc, out coneDir);
        SetConePos();
        PandoraSingleton<MissionManager>.Instance.coneTarget.SetActive(value: true);
        if (unitCtrlr.IsPlayed())
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, unitCtrlr, unitCtrlr.CurrentAction, null);
        }
    }

    void ICheapState.Exit(int iTo)
    {
        unitCtrlr.ClearFlyingTexts();
        PandoraSingleton<MissionManager>.Instance.coneTarget.SetActive(value: false);
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
            unitCtrlr.SendSkillTargets(unitCtrlr.CurrentAction.SkillId, PandoraSingleton<MissionManager>.Instance.coneTarget.transform.position, PandoraSingleton<MissionManager>.Instance.coneTarget.transform.forward);
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
            Vector3 to = lhs * rhs * coneDir;
            if (Vector3.Angle(unitCtrlr.transform.forward, to) < 90f)
            {
                coneDir = to;
            }
        }
        SetConePos();
        unitCtrlr.SetConeTargets(availableTargets, PandoraSingleton<MissionManager>.Instance.coneTarget.transform, highlighTargets: true);
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void SetConePos()
    {
        PandoraSingleton<MissionManager>.Instance.coneTarget.transform.position = coneSrc;
        Quaternion rotation = Quaternion.LookRotation(coneDir);
        PandoraSingleton<MissionManager>.Instance.coneTarget.transform.rotation = rotation;
        unitCtrlr.FaceTarget(PandoraSingleton<MissionManager>.Instance.coneTarget.transform.GetChild(0));
        cam.SetShoulderCam(unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        cam.dummyCam.transform.rotation = rotation;
    }
}
