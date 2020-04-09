using Prometheus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deployment : IMyrtilus, ICheapState
{
	private enum CommandList
	{
		NONE = 0,
		DEPLOY_HERE = 1,
		FORWARD = 2,
		BACKWARD = 3,
		DEPLOY_FINISHED = 4,
		COUNT = 6,
		SPECIFIC_TYPE = 5
	}

	private MissionManager missionMngr;

	private UnitController curUnitCtrl;

	private int spawnNodeIndex = -1;

	private List<SpawnNode> availableNodes;

	private List<GameObject> currentFxs;

	private List<GameObject>[] fxs;

	private int deployIndex;

	private bool impressives;

	private Transform lastCamTarget;

	private GameObject pointerFx;

	private bool done;

	private int synCount;

	public uint uid
	{
		get;
		set;
	}

	public uint owner
	{
		get;
		set;
	}

	public Deployment(MissionManager mission)
	{
		missionMngr = mission;
		RegisterToHermes();
	}

	void ICheapState.Destroy()
	{
		RemoveFromHermes();
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraDebug.LogDebug("Deployment Enter", "FLOW");
		synCount = 0;
		done = false;
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
		{
			for (int i = 0; i < missionMngr.WarbandCtrlrs.Count; i++)
			{
				List<UnitController> unitCtrlrs = missionMngr.WarbandCtrlrs[i].unitCtrlrs;
				for (int j = 0; j < unitCtrlrs.Count; j++)
				{
					unitCtrlrs[j].Imprint.alwaysVisible = missionMngr.WarbandCtrlrs[i].IsPlayed();
					unitCtrlrs[j].Imprint.needsRefresh = true;
				}
			}
			StartRound();
			PandoraSingleton<MissionManager>.Instance.resendLadder = true;
			return;
		}
		PandoraSingleton<MissionManager>.Instance.isDeploying = true;
		lastCamTarget = missionMngr.CamManager.Target;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_DEPLOY);
		deployIndex = -1;
		impressives = true;
		missionMngr.SetTurnTimer(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployTimer, OnTimerDone);
		fxs = new List<GameObject>[missionMngr.WarbandCtrlrs.Count];
		for (int k = 0; k < missionMngr.WarbandCtrlrs.Count; k++)
		{
			fxs[k] = new List<GameObject>();
			List<UnitController> unitCtrlrs2 = missionMngr.WarbandCtrlrs[k].unitCtrlrs;
			for (int l = 0; l < unitCtrlrs2.Count; l++)
			{
				unitCtrlrs2[l].Imprint.alwaysVisible = unitCtrlrs2[l].IsPlayed();
				unitCtrlrs2[l].Imprint.alwaysHide = !unitCtrlrs2[l].IsPlayed();
				if (unitCtrlrs2[l].IsPlayed())
				{
					unitCtrlrs2[l].Imprint.needsRefresh = true;
				}
				else
				{
					unitCtrlrs2[l].Imprint.Hide();
				}
				unitCtrlrs2[l].Hide(!unitCtrlrs2[l].IsPlayed(), force: true);
			}
			availableNodes = PandoraSingleton<MissionStartData>.Instance.spawnNodes[k];
			for (int m = 0; m < availableNodes.Count; m++)
			{
				if (missionMngr.WarbandCtrlrs[k].IsPlayed())
				{
					GameObject gameObject = null;
					availableNodes[m].ShowImprint(isMine: true);
					gameObject = Object.Instantiate(PandoraSingleton<MissionManager>.Instance.deployBeaconPrefab);
					gameObject.transform.SetParent(availableNodes[m].transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					fxs[k].Add(gameObject);
				}
			}
		}
		PandoraSingleton<MissionManager>.Instance.resendLadder = true;
		NextUnitDeploy();
	}

	void ICheapState.Exit(int iTo)
	{
		if (PandoraSingleton<MissionStartData>.Instance.spawnNodes != null)
		{
			for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.spawnNodes.Length; i++)
			{
				List<SpawnNode> list = PandoraSingleton<MissionStartData>.Instance.spawnNodes[i];
				for (int j = 0; j < list.Count; j++)
				{
					SpawnNode spawnNode = list[j];
					if (spawnNode != null)
					{
						Object.DestroyImmediate(spawnNode.gameObject);
					}
				}
			}
			PandoraSingleton<MissionStartData>.Instance.spawnNodes = null;
		}
		if (PandoraSingleton<MissionStartData>.Instance.spawnZones != null)
		{
			for (int k = 0; k < PandoraSingleton<MissionStartData>.Instance.spawnZones.Count; k++)
			{
				SpawnZone spawnZone = PandoraSingleton<MissionStartData>.Instance.spawnZones[k];
				if (spawnZone != null)
				{
					Object.DestroyImmediate(spawnZone.gameObject);
				}
			}
			PandoraSingleton<MissionStartData>.Instance.spawnZones = null;
		}
		availableNodes = null;
		currentFxs = null;
		fxs = null;
		missionMngr.CreateMissionEndData();
		missionMngr.SetDepoyLadderIndex(-1);
		missionMngr.SetTurnTimer(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.turnTimer);
		missionMngr.CamManager.SetZoomLevel(1u);
		missionMngr.InitFoW();
		if (pointerFx != null)
		{
			pointerFx.SetActive(value: false);
			Object.Destroy(pointerFx);
			pointerFx = null;
		}
	}

	void ICheapState.Update()
	{
		if (done)
		{
			return;
		}
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview") && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW)
		{
			missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.OVERVIEW, curUnitCtrl.transform, transition: true, force: true);
		}
		else if ((PandoraSingleton<PandoraInput>.Instance.GetKeyUp("overview", 6) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel", 6)) && missionMngr.CamManager.GetCurrentCamType() == CameraManager.CameraType.OVERVIEW)
		{
			if (curUnitCtrl.IsPlayed())
			{
				missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.DEPLOY, curUnitCtrl.transform, transition: true, force: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
			}
			else
			{
				missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.DEPLOY, lastCamTarget, transition: true, force: true);
			}
		}
		else if (curUnitCtrl != null && missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW && curUnitCtrl.IsPlayed())
		{
			lastCamTarget = curUnitCtrl.transform;
			if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
			{
				Send(true, Hermes.SendTarget.ALL, uid, 2u);
			}
			else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
			{
				Send(true, Hermes.SendTarget.ALL, uid, 3u);
			}
			else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
			{
				Send(true, Hermes.SendTarget.ALL, uid, 1u);
			}
		}
	}

	void ICheapState.FixedUpdate()
	{
		if (!done && curUnitCtrl != null)
		{
			curUnitCtrl.UpdateTargetsData();
			if (curUnitCtrl.IsPlayed())
			{
				PandoraSingleton<MissionManager>.Instance.RefreshFoWOwnMoving(curUnitCtrl);
			}
			else
			{
				PandoraSingleton<MissionManager>.Instance.RefreshFoWTargetMoving(curUnitCtrl);
			}
		}
	}

	private void StartRound()
	{
		done = true;
		missionMngr.StartCoroutine(FinalizeDeployment());
	}

	private IEnumerator FinalizeDeployment()
	{
		for (int l = 0; l < missionMngr.WarbandCtrlrs.Count; l++)
		{
			List<UnitController> units = missionMngr.WarbandCtrlrs[l].unitCtrlrs;
			for (int i = 0; i < units.Count; i++)
			{
				units[i].Deployed(checkEngaged: false);
			}
		}
		for (int k = 0; k < missionMngr.WarbandCtrlrs.Count; k++)
		{
			List<UnitController> units = missionMngr.WarbandCtrlrs[k].unitCtrlrs;
			for (int j = 0; j < units.Count; j++)
			{
				units[j].UpdateTargetsData();
				units[j].CheckEngaged(applyEnchants: false);
				yield return null;
			}
		}
		SendDeploymentFinishedRPC();
	}

	private void SendDeploymentFinishedRPC()
	{
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
		{
			if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex)
			{
				Send(true, Hermes.SendTarget.ALL, uid, 4u);
			}
		}
	}

	private void DeploymentFinishedRPC()
	{
		synCount++;
		if (synCount != PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count)
		{
			return;
		}
		for (int i = 0; i < missionMngr.WarbandCtrlrs.Count; i++)
		{
			List<UnitController> unitCtrlrs = missionMngr.WarbandCtrlrs[i].unitCtrlrs;
			for (int j = 0; j < unitCtrlrs.Count; j++)
			{
				unitCtrlrs[j].TriggerEngagedEnchantments();
				unitCtrlrs[j].TriggerAlliesEnchantments();
			}
		}
		PandoraSingleton<MissionManager>.Instance.isDeploying = false;
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy)
		{
			missionMngr.StateMachine.ChangeState(3);
			PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone();
		}
		else
		{
			missionMngr.StateMachine.ChangeState(2);
		}
	}

	public void OnTimerDone()
	{
		if (curUnitCtrl.IsPlayed())
		{
			Send(true, Hermes.SendTarget.ALL, uid, 1u);
		}
	}

	public void DeployHere()
	{
		availableNodes[spawnNodeIndex].claimed = true;
		if (pointerFx != null)
		{
			pointerFx.SetActive(value: false);
			Object.Destroy(pointerFx);
			pointerFx = null;
		}
		NextUnitDeploy();
	}

	public void NextUnitDeploy()
	{
		if (impressives)
		{
			do
			{
				deployIndex++;
				if (deployIndex >= missionMngr.InitiativeLadder.Count)
				{
					deployIndex = -1;
					impressives = false;
					NextUnitDeploy();
					return;
				}
				curUnitCtrl = missionMngr.InitiativeLadder[deployIndex];
			}
			while (!curUnitCtrl.unit.IsImpressive);
		}
		else
		{
			do
			{
				deployIndex++;
				if (deployIndex >= missionMngr.InitiativeLadder.Count)
				{
					StartRound();
					return;
				}
				curUnitCtrl = missionMngr.InitiativeLadder[deployIndex];
			}
			while (curUnitCtrl.unit.IsImpressive);
		}
		curUnitCtrl = missionMngr.InitiativeLadder[deployIndex];
		missionMngr.SetDepoyLadderIndex(deployIndex);
		PandoraSingleton<UIMissionManager>.Instance.CurrentUnitController = curUnitCtrl;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_CHANGED, curUnitCtrl);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.DEPLOY_UNIT, curUnitCtrl);
		availableNodes = PandoraSingleton<MissionStartData>.Instance.spawnNodes[curUnitCtrl.GetWarband().idx];
		currentFxs = fxs[curUnitCtrl.GetWarband().idx];
		missionMngr.TurnTimer.Reset();
		missionMngr.TurnTimer.Resume();
		spawnNodeIndex = -1;
		FindNextAvailableNode(1, curUnitCtrl.unit.IsImpressive);
		curUnitCtrl.Imprint.alwaysVisible = curUnitCtrl.IsPlayed();
		curUnitCtrl.Imprint.alwaysHide = false;
		curUnitCtrl.Imprint.needsRefresh = true;
		if (curUnitCtrl.IsPlayed())
		{
			if (curUnitCtrl.GetWarband().deploymentId == DeploymentId.SCATTERED)
			{
				DeployAutoUnit();
				return;
			}
			missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.DEPLOY, curUnitCtrl.transform, transition: true, force: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx("fx_arrow_perso_location_01", curUnitCtrl, null, delegate(GameObject fx)
			{
				pointerFx = fx;
			});
		}
		else if (curUnitCtrl.GetWarband().playerTypeId == PlayerTypeId.PLAYER && curUnitCtrl.GetWarband().deploymentId == DeploymentId.SCATTERED)
		{
			DeployAutoUnit();
		}
		else if (curUnitCtrl.AICtrlr != null)
		{
			DeployAutoUnit();
		}
		else if (missionMngr.CamManager.GetCurrentCamType() != CameraManager.CameraType.OVERVIEW)
		{
			missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.DEPLOY, lastCamTarget, transition: true, force: true);
		}
	}

	public void DeployAutoUnit()
	{
		bool isImpressive = curUnitCtrl.unit.IsImpressive;
		bool flag = curUnitCtrl.unit.CampaignData == null && curUnitCtrl.unit.Data.UnitTypeId == UnitTypeId.MONSTER;
		List<int> list = new List<int>();
		for (int i = 0; i < availableNodes.Count; i++)
		{
			if (flag)
			{
				if (availableNodes[i].IsOfType(SpawnNodeId.ROAMING) && !availableNodes[i].claimed)
				{
					list.Add(i);
				}
			}
			else if (isImpressive)
			{
				if (availableNodes[i].IsOfType(SpawnNodeId.IMPRESSIVE) && !availableNodes[i].claimed)
				{
					list.Add(i);
				}
			}
			else if (!availableNodes[i].claimed)
			{
				list.Add(i);
			}
		}
		int index = missionMngr.NetworkTyche.Rand(0, list.Count);
		spawnNodeIndex = list[index];
		if (currentFxs.Count > 0)
		{
			currentFxs[spawnNodeIndex].SetActive(value: false);
		}
		curUnitCtrl.transform.rotation = availableNodes[spawnNodeIndex].transform.rotation;
		curUnitCtrl.SetFixed(availableNodes[spawnNodeIndex].transform.position, fix: true);
		DeployHere();
		list.Clear();
	}

	public void FindNextAvailableNode(int dir, bool impressive)
	{
		int num = spawnNodeIndex;
		int num2 = spawnNodeIndex;
		bool flag = false;
		while (!flag)
		{
			num2 += dir;
			if (num2 >= availableNodes.Count)
			{
				num2 = 0;
			}
			else if (num2 < 0)
			{
				num2 = availableNodes.Count - 1;
			}
			if (num == num2)
			{
				flag = true;
			}
			if (!availableNodes[num2].claimed && ((availableNodes[num2].IsOfType(SpawnNodeId.IMPRESSIVE) && impressive) || !impressive))
			{
				break;
			}
		}
		SelectSpecificNode(num2);
	}

	public void RegisterToHermes()
	{
		PandoraSingleton<Hermes>.Instance.RegisterMyrtilus(this);
	}

	public void RemoveFromHermes()
	{
		PandoraSingleton<Hermes>.Instance.RemoveMyrtilus(this);
	}

	public void Send(bool reliable, Hermes.SendTarget target, uint id, uint command, params object[] parms)
	{
		PandoraSingleton<Hermes>.Instance.Send(reliable, target, id, command, parms);
	}

	public void Receive(ulong from, uint command, object[] parms)
	{
		switch (command)
		{
		case 1u:
			DeployHere();
			break;
		case 2u:
			FindNextAvailableNode(1, curUnitCtrl.unit.IsImpressive);
			break;
		case 3u:
			FindNextAvailableNode(-1, curUnitCtrl.unit.IsImpressive);
			break;
		case 4u:
			DeploymentFinishedRPC();
			break;
		case 5u:
			SelectSpecificNode((int)parms[0]);
			break;
		}
	}

	public void SelectSpecificNode(int nodeId)
	{
		int num = spawnNodeIndex;
		spawnNodeIndex = nodeId;
		if (currentFxs.Count > 0)
		{
			if (num != -1 && currentFxs[num] != null)
			{
				currentFxs[num].SetActive(value: true);
			}
			currentFxs[spawnNodeIndex].SetActive(value: false);
		}
		curUnitCtrl.transform.rotation = availableNodes[spawnNodeIndex].transform.rotation;
		curUnitCtrl.SetFixed(availableNodes[spawnNodeIndex].transform.position, fix: true);
		if (curUnitCtrl.IsPlayed())
		{
			PandoraSingleton<MissionManager>.Instance.CamManager.Transition();
		}
	}

	public void RecenterCameraOnDeployedUnit()
	{
		missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.DEPLOY, curUnitCtrl.transform, transition: true, force: true, curUnitCtrl.unit.Data.UnitSizeId == UnitSizeId.LARGE);
	}

	public int FindSpawnNodeIndex(SpawnNode node)
	{
		for (int i = 0; i < availableNodes.Count; i++)
		{
			if (node == availableNodes[i])
			{
				return i;
			}
		}
		return -1;
	}
}
