using System.Collections.Generic;
using UnityEngine;

public class WatchUnit : ICheapState
{
    private const float SEE_TIME = 0.5f;

    private const float NO_SEE_TIME = -1f;

    private MissionManager missionMngr;

    private List<UnitController> units;

    private UnitController curUnitCtrl;

    private int curUnit;

    private float seeTime;

    private WatchCamera watchCam;

    public WatchUnit(MissionManager mission)
    {
        missionMngr = mission;
    }

    void ICheapState.Destroy()
    {
        missionMngr = null;
    }

    void ICheapState.Enter(int iFrom)
    {
        units = missionMngr.GetMyAliveUnits();
        if (units.Count == 0)
        {
            units = missionMngr.GetAllMyUnits();
        }
        curUnit = 0;
        curUnitCtrl = units[curUnit];
        curUnitCtrl = PandoraSingleton<MissionManager>.Instance.GetLastPlayedAliveUnit(units[curUnit].unit.warbandIdx);
        for (int i = 0; i < units.Count; i++)
        {
            if (curUnitCtrl == units[i])
            {
                curUnit = i;
                break;
            }
        }
        if (curUnitCtrl == null)
        {
            curUnitCtrl = units[curUnit];
        }
        watchCam = missionMngr.CamManager.GetCamOfType<WatchCamera>(CameraManager.CameraType.WATCH);
        if (PandoraSingleton<MissionManager>.Instance.lastWarbandIdx == -1 || PandoraSingleton<MissionManager>.Instance.lastWarbandIdx == PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().idx)
        {
            if (missionMngr.GetCurrentUnit().IsImprintVisible())
            {
                missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.WATCH, null, transition: true, force: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
            }
            else
            {
                missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, curUnitCtrl.transform, transition: true, force: true, clearFocus: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
            }
        }
        seeTime = 1f;
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
        if (missionMngr.CheckUnitTurnFinished())
        {
            return;
        }
        if (missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.WATCH && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.ANIMATED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.CONSTRAINED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.SEMI_CONSTRAINED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.MELEE_ATTACK)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview") && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW)
            {
                missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.OVERVIEW, curUnitCtrl.transform, transition: true, force: true);
            }
            else if ((PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview", 6) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel", 6)) && missionMngr.CamManager.GetCurrentCamType() == CameraManager.CameraType.OVERVIEW)
            {
                missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, curUnitCtrl.transform, transition: true, force: true, clearFocus: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
            }
            else if (units.Count > 1 && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW)
            {
                if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
                {
                    if (++curUnit >= units.Count)
                    {
                        curUnit = 0;
                    }
                    curUnitCtrl = units[curUnit];
                    missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, curUnitCtrl.transform, transition: true, force: true, clearFocus: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
                }
                else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
                {
                    if (--curUnit < 0)
                    {
                        curUnit = units.Count - 1;
                    }
                    curUnitCtrl = units[curUnit];
                    missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, curUnitCtrl.transform, transition: true, force: true, clearFocus: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
                }
            }
        }
        if (missionMngr.GetCurrentUnit().IsImprintVisible())
        {
            if (seeTime < -0.1f)
            {
                seeTime = 0f;
            }
            seeTime += Time.deltaTime;
        }
        else if (seeTime > 0.1f)
        {
            seeTime = 0f;
        }
        else
        {
            seeTime -= Time.deltaTime;
        }
        if (seeTime > 0.5f && ((missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.ANIMATED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.CONSTRAINED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.SEMI_CONSTRAINED && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.MELEE_ATTACK) || !PandoraSingleton<SequenceManager>.Instance.isPlaying))
        {
            if (((missionMngr.CamManager.GetCurrentCamType() == CameraManager.CameraType.OVERVIEW && PandoraSingleton<GameManager>.Instance.AutoExitTacticalEnabled) || missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW) && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.WATCH)
            {
                missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.WATCH, null, transition: true, force: true);
            }
        }
        else
        {
            if (!(seeTime < -1f) || missionMngr.CamManager.GetCurrentCamType() == CameraManager.CameraType.OVERVIEW || missionMngr.CamManager.GetCurrentCamType() == CameraManager.CameraType.CHARACTER)
            {
                return;
            }
            UnitController lastWatcher = watchCam.lastWatcher;
            for (int i = 0; i < units.Count; i++)
            {
                if (lastWatcher == units[i])
                {
                    curUnit = i;
                    curUnitCtrl = units[i];
                    break;
                }
            }
            missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, curUnitCtrl.transform, transition: true, force: true, clearFocus: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        }
    }

    void ICheapState.FixedUpdate()
    {
    }
}
