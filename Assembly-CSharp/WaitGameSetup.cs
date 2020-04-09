using UnityEngine;

public class WaitGameSetup : ICheapState
{
	private MissionManager missionMngr;

	public WaitGameSetup(MissionManager mission)
	{
		missionMngr = mission;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		Transform camTarget = null;
		UnitController unitController = null;
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
		{
			for (int i = 0; i < missionMngr.InitiativeLadder.Count; i++)
			{
				if (missionMngr.InitiativeLadder[i].IsPlayed())
				{
					unitController = missionMngr.InitiativeLadder[i];
					camTarget = unitController.transform;
					break;
				}
			}
		}
		else
		{
			camTarget = PandoraSingleton<MissionStartData>.Instance.spawnNodes[missionMngr.GetMyWarbandCtrlr().idx][0].transform;
		}
		missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, camTarget, transition: false, force: true, clearFocus: true, (bool)unitController && unitController.unit.Data.UnitSizeId == UnitSizeId.LARGE);
		missionMngr.CamManager.GetComponent<Camera>().enabled = true;
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}
}
