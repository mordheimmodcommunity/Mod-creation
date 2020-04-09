using System.Collections;
using System.Collections.Generic;

public class StartRound : ICheapState
{
	private MissionManager missionMngr;

	private List<CampaignMissionSpawnData> spawnsData;

	private List<WarbandController> teamWarbands = new List<WarbandController>();

	public StartRound(MissionManager mission)
	{
		missionMngr = mission;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		missionMngr.CombatLogger.AddLog(CombatLogger.LogMessage.ROUND_START, (missionMngr.currentTurn + 1).ToConstantString());
		missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.FIXED, null);
		if (missionMngr.campaignId != 0 && !PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			missionMngr.StartCoroutine(CheckReinforcements());
		}
		else
		{
			StartRoundAction();
		}
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

	private IEnumerator CheckReinforcements()
	{
		bool noticeSent = false;
		int totalUnitSpawned = 0;
		if (spawnsData == null)
		{
			spawnsData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionSpawnData>("fk_campaign_mission_id", ((int)missionMngr.campaignId).ToConstantString());
		}
		for (int i = 0; i < spawnsData.Count; i++)
		{
			int teamIdx = spawnsData[i].Team;
			teamWarbands.Clear();
			int aliveUnits = 0;
			for (int w2 = 0; w2 < missionMngr.WarbandCtrlrs.Count; w2++)
			{
				if (missionMngr.WarbandCtrlrs[w2].teamIdx == teamIdx)
				{
					teamWarbands.Add(missionMngr.WarbandCtrlrs[w2]);
					for (int u = 0; u < missionMngr.WarbandCtrlrs[w2].unitCtrlrs.Count; u++)
					{
						aliveUnits += ((missionMngr.WarbandCtrlrs[w2].unitCtrlrs[u].unit.Status != UnitStateId.OUT_OF_ACTION) ? 1 : 0);
					}
				}
			}
			if (aliveUnits >= spawnsData[i].MinUnit)
			{
				continue;
			}
			List<DecisionPoint> availableSpawnPoints = missionMngr.GetAvailableSpawnPoints(visible: false, asc: true);
			int requiredUnits = spawnsData[i].MinUnit - aliveUnits;
			int spawnedUnits = 0;
			List<CampaignMissionSpawnQueueData> queuesData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionSpawnQueueData>("fk_campaign_mission_spawn_id", ((int)spawnsData[i].Id).ToConstantString());
			queuesData.Sort((CampaignMissionSpawnQueueData x, CampaignMissionSpawnQueueData y) => x.Order.CompareTo(y.Order));
			for (int q = 0; q < queuesData.Count; q++)
			{
				if (spawnedUnits >= requiredUnits)
				{
					break;
				}
				int queueSpawned = 0;
				List<UnitController> unitTypeSrcs = new List<UnitController>();
				for (int w = 0; w < teamWarbands.Count; w++)
				{
					for (int u2 = 0; u2 < teamWarbands[w].unitCtrlrs.Count; u2++)
					{
						if (teamWarbands[w].unitCtrlrs[u2].unit.Data.UnitTypeId == queuesData[q].UnitTypeId)
						{
							unitTypeSrcs.Add(teamWarbands[w].unitCtrlrs[u2]);
						}
					}
				}
				while (unitTypeSrcs.Count > 0 && availableSpawnPoints.Count > 0 && queueSpawned < queuesData[q].Amount && spawnedUnits < requiredUnits)
				{
					if (!noticeSent)
					{
						noticeSent = true;
						PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_REINFORCEMENTS, missionMngr.currentTurn);
					}
					spawnedUnits++;
					queueSpawned++;
					totalUnitSpawned++;
					UnitController srcCtrlr = unitTypeSrcs[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, unitTypeSrcs.Count)];
					DecisionPoint spawnPoint = availableSpawnPoints[0];
					availableSpawnPoints.RemoveAt(0);
					yield return missionMngr.StartCoroutine(PandoraSingleton<UnitFactory>.Instance.CloneUnitCtrlr(srcCtrlr, queuesData[q].Rank, queuesData[q].Rating, spawnPoint.transform.position, spawnPoint.transform.rotation));
				}
			}
		}
		StartRoundAction();
	}

	private void StartRoundAction()
	{
		if (PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			missionMngr.currentTurn = PandoraSingleton<MissionStartData>.Instance.currentTurn;
			missionMngr.ReloadLadder();
			for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.morals.Count; i++)
			{
				WarbandController warbandController = missionMngr.WarbandCtrlrs[i];
				warbandController.idolMoralRemoved = PandoraSingleton<MissionStartData>.Instance.morals[i].Item3;
				warbandController.OldMoralValue = PandoraSingleton<MissionStartData>.Instance.morals[i].Item2;
				warbandController.MoralValue = PandoraSingleton<MissionStartData>.Instance.morals[i].Item1;
			}
		}
		else if (missionMngr.currentTurn != 0)
		{
			missionMngr.ResetLadderIdx();
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_ROUND_START, missionMngr.currentTurn);
		}
		for (int j = 0; j < missionMngr.WarbandCtrlrs.Count; j++)
		{
			WarbandController warbandController2 = missionMngr.WarbandCtrlrs[j];
			for (int k = 0; k < warbandController2.unitCtrlrs.Count; k++)
			{
				UnitController unitController = warbandController2.unitCtrlrs[k];
				if (unitController.StateMachine.GetActiveStateId() != 39 && missionMngr.currentTurn != 0)
				{
					continue;
				}
				if (unitController.unit.IsAvailable())
				{
					if (unitController.unit.OverwatchLeft > 0 && unitController.HasRange())
					{
						unitController.StateMachine.ChangeState(36);
						continue;
					}
					if (unitController.unit.AmbushLeft > 0 && unitController.HasClose())
					{
						unitController.StateMachine.ChangeState(37);
						continue;
					}
				}
				unitController.StateMachine.ChangeState(9);
			}
			if (warbandController2.SquadManager != null)
			{
				warbandController2.SquadManager.RefreshSquads();
			}
		}
		if (!PandoraSingleton<MissionStartData>.Instance.isReload)
		{
			missionMngr.RestoreUnitWeapons();
		}
		if (missionMngr.DissolveDeadUnits())
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_DEAD_UNIT_FLEE, missionMngr.currentTurn);
		}
		missionMngr.UpdateObjectivesUI();
		missionMngr.SendTurnReady();
	}
}
