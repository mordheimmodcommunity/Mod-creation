using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionStartData : PandoraSingleton<MissionStartData>, IMyrtilus
{
	private enum CommandList
	{
		NONE,
		ADD_WARBAND,
		SET_WARBAND,
		SET_MISSION,
		KICK_PLAYER,
		SET_READY,
		COUNT
	}

	public bool isReload;

	public List<uint> usedTraps = new List<uint>();

	public List<KeyValuePair<uint, SearchSave>> searches = new List<KeyValuePair<uint, SearchSave>>();

	public List<EndZoneAoe> aoeZones = new List<EndZoneAoe>();

	public List<MissionEndUnitSave> units = new List<MissionEndUnitSave>();

	public List<Tuple<int, int, bool>> morals = new List<Tuple<int, int, bool>>();

	public List<KeyValuePair<int, int>> reinforcementsIdx = new List<KeyValuePair<int, int>>();

	public List<KeyValuePair<uint, uint>> objectives = new List<KeyValuePair<uint, uint>>();

	public List<KeyValuePair<uint, int>> converters = new List<KeyValuePair<uint, int>>();

	public List<KeyValuePair<uint, bool>> activaters = new List<KeyValuePair<uint, bool>>();

	public List<EndDynamicTrap> dynamicTraps = new List<EndDynamicTrap>();

	public List<EndDestructible> destructibles = new List<EndDestructible>();

	public List<uint> myrtilusLadder;

	public int currentLadderIdx;

	public int currentTurn;

	public List<SpawnNode>[] spawnNodes;

	public List<SpawnZone> spawnZones;

	private bool locked;

	public int Seed
	{
		get;
		private set;
	}

	public Mission CurrentMission
	{
		get;
		private set;
	}

	public List<MissionWarbandSave> FightingWarbands
	{
		get;
		private set;
	}

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

	public bool IsLocked => locked;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		Clear();
	}

	public void Clear()
	{
		CurrentMission = new Mission(new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE)));
		FightingWarbands = new List<MissionWarbandSave>();
		ResetSeed();
		isReload = false;
		locked = false;
		usedTraps.Clear();
		searches.Clear();
		aoeZones.Clear();
		units.Clear();
		morals.Clear();
		reinforcementsIdx.Clear();
		objectives.Clear();
		converters.Clear();
		activaters.Clear();
		dynamicTraps.Clear();
		destructibles.Clear();
		myrtilusLadder = new List<uint>();
		currentLadderIdx = 0;
		currentTurn = 0;
	}

	public void ResetSeed()
	{
		Seed = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, int.MaxValue);
	}

	public void Lock()
	{
		locked = true;
	}

	public void ResetWarbandsReady()
	{
		for (int i = 1; i < FightingWarbands.Count; i++)
		{
			FightingWarbands[i].ResetReady();
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_READY);
	}

	public void InitSkirmish(WarbandMenuController warband, List<int> unitsPosition, bool isExhibition)
	{
		ResetSeed();
		SetMission(Mission.GenerateSkirmishMission());
		FightingWarbands.Clear();
		int rating = 0;
		warband.GetSkirmishInfo(unitsPosition, out rating, out string[] serializedUnits);
		AddFightingWarband(warband.Warband.Id, CampaignWarbandId.NONE, warband.Warband.GetWarbandSave().name, warband.Warband.GetWarbandSave().overrideName, PandoraSingleton<Hephaestus>.Instance.GetUserName(), warband.Warband.Rank, rating, PandoraSingleton<Hermes>.Instance.PlayerIndex, PlayerTypeId.PLAYER, serializedUnits);
		FightingWarbands[0].IsReady = true;
		CurrentMission.missionSave.isSkirmish = isExhibition;
	}

	public void RegenerateMission(MissionMapId mapId = MissionMapId.NONE, DeploymentScenarioId scenarioId = DeploymentScenarioId.NONE, bool keepLayout = false)
	{
		bool randomLayout = false;
		int mapLayoutId = 0;
		bool randomGameplay = false;
		int mapGameplayId = 0;
		if (keepLayout)
		{
			randomLayout = CurrentMission.missionSave.randomLayout;
			mapLayoutId = CurrentMission.missionSave.mapLayoutId;
			randomGameplay = CurrentMission.missionSave.randomGameplay;
			mapGameplayId = CurrentMission.missionSave.mapGameplayId;
		}
		int turnTimer = 0;
		if (CurrentMission != null)
		{
			turnTimer = CurrentMission.missionSave.turnTimer;
		}
		int deployTimer = 0;
		if (CurrentMission != null)
		{
			deployTimer = CurrentMission.missionSave.deployTimer;
		}
		int beaconLimit = 0;
		if (CurrentMission != null)
		{
			beaconLimit = CurrentMission.missionSave.beaconLimit;
		}
		bool autoDeploy = false;
		if (CurrentMission != null)
		{
			autoDeploy = CurrentMission.missionSave.autoDeploy;
		}
		bool randomRoaming = false;
		if (CurrentMission != null)
		{
			randomRoaming = CurrentMission.missionSave.randomRoaming;
		}
		int roamingUnitId = 0;
		if (CurrentMission != null)
		{
			roamingUnitId = CurrentMission.missionSave.roamingUnitId;
		}
		bool isSkirmish = true;
		if (CurrentMission != null)
		{
			isSkirmish = CurrentMission.missionSave.isSkirmish;
		}
		float routThreshold = Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE);
		if (CurrentMission != null)
		{
			routThreshold = CurrentMission.missionSave.routThreshold;
		}
		int ratingId = 1;
		if (CurrentMission != null)
		{
			ratingId = CurrentMission.missionSave.ratingId;
		}
		SetMission(Mission.GenerateSkirmishMission(mapId, scenarioId));
		if (keepLayout)
		{
			CurrentMission.missionSave.randomLayout = randomLayout;
			CurrentMission.missionSave.mapLayoutId = mapLayoutId;
			CurrentMission.missionSave.randomGameplay = randomGameplay;
			CurrentMission.missionSave.mapGameplayId = mapGameplayId;
		}
		CurrentMission.missionSave.turnTimer = turnTimer;
		CurrentMission.missionSave.deployTimer = deployTimer;
		CurrentMission.missionSave.beaconLimit = beaconLimit;
		CurrentMission.missionSave.autoDeploy = autoDeploy;
		CurrentMission.missionSave.randomRoaming = randomRoaming;
		CurrentMission.missionSave.roamingUnitId = roamingUnitId;
		CurrentMission.missionSave.isSkirmish = isSkirmish;
		CurrentMission.missionSave.routThreshold = routThreshold;
		CurrentMission.missionSave.ratingId = ratingId;
		SendMission(clearWarbands: false);
	}

	public IEnumerator SetMissionFull(Mission mission, WarbandMenuController warbandMenuCtrlr, Action callback)
	{
		SetMission(mission);
		FightingWarbands.Clear();
		if (mission.missionSave.isCampaign)
		{
			if (mission.missionSave.campaignId != 0)
			{
				List<CampaignMissionJoinCampaignWarbandData> campaignWarbandsData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionJoinCampaignWarbandData>("fk_campaign_mission_id", mission.missionSave.campaignId.ToString());
				for (int k = 0; k < campaignWarbandsData.Count; k++)
				{
					CampaignMissionJoinCampaignWarbandData warData = campaignWarbandsData[k];
					if (warData.CampaignWarbandId == CampaignWarbandId.NONE)
					{
						AddFightingWarband(warbandMenuCtrlr, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_ai"), PlayerTypeId.PLAYER);
						continue;
					}
					WarbandMenuController ctrlr = new WarbandMenuController(Mission.GetCampaignWarband(warData));
					AddFightingWarband(ctrlr, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_ai"), warData.PlayerTypeId);
				}
			}
		}
		else
		{
			AddFightingWarband(warbandMenuCtrlr, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_ai"), PlayerTypeId.PLAYER);
			for (int j = 1; j < mission.missionSave.deployCount; j++)
			{
				bool impressive = false;
				int heroesCount = 0;
				int highestUnitRank = 0;
				for (int i = 0; i < FightingWarbands[0].Units.Count; i++)
				{
					highestUnitRank = Mathf.Max(highestUnitRank, PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)FightingWarbands[0].Units[j].rankId).Rank);
					int id = FightingWarbands[0].Units[i].stats.id;
					switch (Unit.GetUnitTypeId(baseUnitTypeId: PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(id).UnitTypeId, save: FightingWarbands[0].Units[i]))
					{
					case UnitTypeId.IMPRESSIVE:
						impressive = true;
						break;
					case UnitTypeId.HERO_1:
					case UnitTypeId.HERO_2:
					case UnitTypeId.HERO_3:
						heroesCount++;
						break;
					}
				}
				int warbandRating = warbandMenuCtrlr.Warband.GetRating();
				int newWarbandRating2 = warbandRating;
				if (mission.missionSave.ratingId != 0)
				{
					ProcMissionRatingData procMissionRatingData2 = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>(mission.missionSave.ratingId);
					int ratingPerc2 = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(procMissionRatingData2.ProcMinValue, procMissionRatingData2.ProcMaxValue);
					newWarbandRating2 += warbandRating * ratingPerc2 / 100;
				}
				else if (mission.missionSave.rating >= 0)
				{
					newWarbandRating2 = ((mission.missionSave.rating >= 100) ? (newWarbandRating2 + mission.missionSave.rating) : (newWarbandRating2 + warbandRating * mission.missionSave.rating / 100));
				}
				else
				{
					ProcMissionRatingData procMissionRatingData = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>(-mission.missionSave.rating);
					int ratingPerc = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(procMissionRatingData.ProcMinValue, procMissionRatingData.ProcMaxValue);
					newWarbandRating2 += warbandRating * ratingPerc / 100;
				}
				yield return StartCoroutine(Mission.GetProcWarband(warData: warbandMenuCtrlr.Warband.GetNextNotFacedWarband(PandoraSingleton<GameManager>.Instance.LocalTyche), rating: newWarbandRating2, warRank: warbandMenuCtrlr.Warband.Rank, warUnitsCount: FightingWarbands[0].Units.Count, impressive: impressive, heroesCount: heroesCount, highestUnitRank: highestUnitRank, callback: delegate(WarbandSave save)
				{
					WarbandMenuController warbandMenuController = new WarbandMenuController(save);
					AddFightingWarband(warbandMenuController, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_ai"), PlayerTypeId.AI);
					base.mission.missionSave.rating = warbandMenuController.Warband.GetRating();
					int num = 0;
					for (int l = 0; l < base.warbandMenuCtrlr.unitCtrlrs.Count; l++)
					{
						if (base.warbandMenuCtrlr.unitCtrlrs[l].unit.UnitSave.warbandSlotIndex < 12 && base.warbandMenuCtrlr.unitCtrlrs[l].unit.GetActiveStatus() == UnitActiveStatusId.AVAILABLE)
						{
							num = ((base.warbandMenuCtrlr.unitCtrlrs[l].unit.Rank <= num) ? num : base.warbandMenuCtrlr.unitCtrlrs[l].unit.Rank);
						}
					}
					if (num <= Constant.GetInt(ConstantId.ROAMING_MIN_RANK) || base.mission.missionSave.ratingId < 3)
					{
						base.mission.missionSave.roamingUnitId = 0;
					}
				}));
			}
		}
		callback();
	}

	public void AddFightingWarband(WarbandMenuController ctrlr, string playerName, PlayerTypeId playerType)
	{
		AddFightingWarband((WarbandId)ctrlr.Warband.GetWarbandSave().id, (CampaignWarbandId)ctrlr.Warband.GetWarbandSave().campaignId, ctrlr.Warband.GetWarbandSave().name, ctrlr.Warband.GetWarbandSave().overrideName, playerName, ctrlr.Warband.Rank, ctrlr.Warband.GetRating(), PandoraSingleton<Hermes>.Instance.PlayerIndex, playerType, ctrlr.GetActiveUnitsSerialized());
	}

	public void AddFightingWarband(WarbandId type, CampaignWarbandId campaignId, string name, string overrideName, string playerName, int rank, int rating, int playerIndex, PlayerTypeId playerTypeId, string[] units)
	{
		FightingWarbands.Add(new MissionWarbandSave(type, campaignId, name, overrideName, playerName, rank, rating, playerIndex, playerTypeId, units));
	}

	public void SetFightingWarband(int warbandMissionIdx, WarbandId type, string name, string overrideName, string playerName, int rank, int rating, int playerIndex, PlayerTypeId playerTypeId, string[] units)
	{
		FightingWarbands[warbandMissionIdx] = new MissionWarbandSave(type, CampaignWarbandId.NONE, name, overrideName, playerName, rank, rating, playerIndex, playerTypeId, units);
		if (warbandMissionIdx == 0)
		{
			FightingWarbands[warbandMissionIdx].IsReady = true;
		}
	}

	public void SetMission(Mission mission)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < mission.missionSave.deployCount; i++)
		{
			string text3 = text;
			text = text3 + " Deploy " + i + " : " + (DeploymentScenarioSlotId)mission.missionSave.deployScenarioSlotIds[i];
			text3 = text2;
			text2 = text3 + " Objective " + i + " : " + (PrimaryObjectiveTypeId)mission.missionSave.objectiveTypeIds[i];
		}
		PandoraDebug.LogInfo("Set mission  seed = " + Seed + " deployCount = " + mission.missionSave.deployScenarioSlotIds.Count + " deployScenarioMapLayoutId = " + (DeploymentScenarioMapLayoutId)mission.missionSave.deployScenarioMapLayoutId + " mapLayoutId = " + (MissionMapLayoutId)mission.missionSave.mapLayoutId + " VictoryTypeId = " + (VictoryTypeId)mission.missionSave.VictoryTypeId + " turnTimer = " + mission.missionSave.turnTimer + " deployTimer = " + mission.missionSave.deployTimer + text + text2 + " wyrdPlacementId = " + (WyrdstonePlacementId)mission.missionSave.wyrdPlacementId + " wyrdDensityId = " + (WyrdstoneDensityId)mission.missionSave.wyrdDensityId + " searchDensity = " + (SearchDensityId)mission.missionSave.searchDensityId, "HERMES");
		CurrentMission = mission;
	}

	public int GetWarbandIndex(int hermesIdx)
	{
		for (int i = 0; i < FightingWarbands.Count; i++)
		{
			if (FightingWarbands[i].PlayerIndex == hermesIdx)
			{
				return i;
			}
		}
		return -1;
	}

	public void ReloadMission(MissionEndDataSave endmission, WarbandSave playerWarband)
	{
		isReload = true;
		Seed = endmission.seed;
		usedTraps = endmission.destroyedTraps;
		searches = endmission.searches;
		aoeZones = endmission.aoeZones;
		units = endmission.units;
		myrtilusLadder = endmission.myrtilusLadder;
		currentLadderIdx = endmission.currentLadderIdx;
		currentTurn = endmission.currentTurn;
		morals = endmission.warbandMorals;
		reinforcementsIdx = endmission.reinforcements;
		objectives = endmission.objectives;
		converters = endmission.converters;
		activaters = endmission.activaters;
		dynamicTraps = endmission.dynamicTraps;
		destructibles = endmission.destructibles;
		endmission.missionSave.autoDeploy = true;
		SetMission(new Mission(endmission.missionSave));
		FightingWarbands.Clear();
		FightingWarbands = endmission.missionWarbands;
	}

	public void SendMissionStartData()
	{
		SendMission(clearWarbands: true);
		for (int i = 0; i < FightingWarbands.Count; i++)
		{
			MissionWarbandSave missionWarbandSave = FightingWarbands[i];
			PandoraDebug.LogInfo("Send SetWarband type = " + missionWarbandSave.WarbandId + " name = " + missionWarbandSave.Name + " player = " + missionWarbandSave.PlayerIndex + " player type = " + missionWarbandSave.PlayerTypeId + " units = " + missionWarbandSave.Units.Count, "HERMES");
			Send(true, Hermes.SendTarget.OTHERS, uid, 2u, i, (int)missionWarbandSave.WarbandId, missionWarbandSave.Name, missionWarbandSave.OverrideName, missionWarbandSave.PlayerName, missionWarbandSave.Rank, missionWarbandSave.Rating, missionWarbandSave.PlayerIndex, (int)missionWarbandSave.PlayerTypeId, missionWarbandSave.SerializedUnits);
		}
	}

	public void RefreshMyWarband(WarbandMenuController warbandCtrlr, List<int> unitsPosition)
	{
		int num = 0;
		MissionWarbandSave missionWarbandSave;
		while (true)
		{
			if (num < FightingWarbands.Count)
			{
				missionWarbandSave = FightingWarbands[num];
				if (missionWarbandSave.PlayerIndex == PandoraSingleton<Hermes>.Instance.PlayerIndex && missionWarbandSave.PlayerTypeId == PlayerTypeId.PLAYER)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		warbandCtrlr.GetSkirmishInfo(unitsPosition, out int rating, out string[] serializedUnits);
		Send(true, Hermes.SendTarget.ALL, uid, 2u, num, (int)missionWarbandSave.WarbandId, missionWarbandSave.Name, missionWarbandSave.OverrideName, missionWarbandSave.PlayerName, missionWarbandSave.Rank, rating, missionWarbandSave.PlayerIndex, (int)missionWarbandSave.PlayerTypeId, serializedUnits);
		ResetWarbandsReady();
	}

	public void SendMission(bool clearWarbands)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < CurrentMission.missionSave.deployCount; i++)
		{
			string text3 = text;
			text = text3 + " Deploy " + i + " : " + (DeploymentScenarioSlotId)CurrentMission.missionSave.deployScenarioSlotIds[i];
			text3 = text2;
			text2 = text3 + " Objective " + i + " : " + (PrimaryObjectiveTypeId)CurrentMission.missionSave.objectiveTypeIds[i];
		}
		PandoraDebug.LogInfo("SendMission  seed = " + Seed + " deployCount = " + CurrentMission.missionSave.deployCount + " deployScenarioMapLayoutId = " + (DeploymentScenarioMapLayoutId)CurrentMission.missionSave.deployScenarioMapLayoutId + " mapLayoutId = " + (MissionMapLayoutId)CurrentMission.missionSave.mapLayoutId + " VictoryTypeId = " + (VictoryTypeId)CurrentMission.missionSave.VictoryTypeId + " turnTimer = " + CurrentMission.missionSave.turnTimer + " deployTimer = " + CurrentMission.missionSave.deployTimer + text + text2 + " wyrdPlacementId = " + (WyrdstonePlacementId)CurrentMission.missionSave.wyrdPlacementId + " wyrdDensityId = " + (WyrdstoneDensityId)CurrentMission.missionSave.wyrdDensityId + " searchDensity = " + (SearchDensityId)CurrentMission.missionSave.searchDensityId + " routThreshold = " + CurrentMission.missionSave.routThreshold, "HERMES");
		Send(true, Hermes.SendTarget.OTHERS, uid, 3u, Seed, Thoth.WriteToString(CurrentMission.missionSave), clearWarbands);
		ResetWarbandsReady();
	}

	private void MissionRPC(int seed, string serializedMission, bool clear)
	{
		Seed = seed;
		MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
		Thoth.ReadFromString(serializedMission, missionSave);
		SetMission(new Mission(missionSave));
		if (clear)
		{
			FightingWarbands = new List<MissionWarbandSave>();
			for (int i = 0; i < CurrentMission.missionSave.deployCount; i++)
			{
				FightingWarbands.Add(null);
			}
		}
		else
		{
			ResetWarbandsReady();
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_UPDATED);
	}

	private void SetWarbandInfoRPC(int missionWarbandIdx, int warbandId, string name, string overrideName, string playerName, int rank, int rating, int playerIndex, int playerTypeId, string[] unitSaves)
	{
		PandoraDebug.LogInfo("Client received WarbandInfoRPC : type = " + (WarbandId)warbandId + " name = " + name + " overridename = " + overrideName + " playerName = " + playerName + " player = " + playerIndex + " playerType = " + (PlayerTypeId)playerTypeId + "HERMES");
		SetFightingWarband(missionWarbandIdx, (WarbandId)warbandId, name, overrideName, playerName, rank, rating, playerIndex, (PlayerTypeId)playerTypeId, unitSaves);
		if (playerIndex > 0)
		{
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_JOIN);
		}
		if (playerIndex != PandoraSingleton<Hermes>.Instance.PlayerIndex)
		{
			CurrentMission.missionSave.rating = rating;
			CurrentMission.RefreshDifficulty(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRating(), isProc: false);
		}
	}

	public void OnNetworkConnected(WarbandMenuController warbandCtrlr, List<int> unitsPosition)
	{
		warbandCtrlr.GetSkirmishInfo(unitsPosition, out int rating, out string[] serializedUnits);
		PandoraDebug.LogInfo("SendWarbandInfo type = " + warbandCtrlr.Warband.Id + " name = " + warbandCtrlr.Warband.GetWarbandSave().Name + " player = " + PandoraSingleton<Hermes>.Instance.PlayerIndex + " team = " + PandoraSingleton<Hermes>.Instance.PlayerIndex + " player type = " + 2 + " units = " + warbandCtrlr.GetActiveUnitsSave().Count, "HERMES");
		Send(true, Hermes.SendTarget.OTHERS, uid, 1u, (int)warbandCtrlr.Warband.Id, warbandCtrlr.Warband.GetWarbandSave().name, warbandCtrlr.Warband.GetWarbandSave().overrideName, PandoraSingleton<Hephaestus>.Instance.GetUserName(), warbandCtrlr.Warband.Rank, rating, PandoraSingleton<Hermes>.Instance.PlayerIndex, 2, serializedUnits);
	}

	private void AddWarbandInfoRPC(int warbandId, string name, string overrideName, string playerName, int rank, int rating, int playerIndex, int playerTypeId, string[] unitSaves)
	{
		int count = FightingWarbands.Count;
		PandoraDebug.LogInfo("Server received WarbandInfoRPC : WarbandIdx = " + count + "type = " + (WarbandId)warbandId + " name = " + name + " overridename = " + overrideName + " playerName = " + playerName + " rank = " + rank + " rating = " + rating + " player = " + playerIndex + " playerType = " + (PlayerTypeId)playerTypeId + " units = " + unitSaves, "HERMES");
		if (count == 2)
		{
			PandoraDebug.LogInfo("Server already have 2 player, this player should be kick soon", "HERMES");
			return;
		}
		AddFightingWarband((WarbandId)warbandId, CampaignWarbandId.NONE, name, overrideName, playerName, rank, rating, playerIndex, (PlayerTypeId)playerTypeId, unitSaves);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_JOIN);
		SendMissionStartData();
	}

	public void SendReady(bool ready)
	{
		Send(true, Hermes.SendTarget.ALL, uid, 5u, PandoraSingleton<Hermes>.Instance.PlayerIndex, ready);
	}

	private void ReadyRPC(int hermesPlayerIdx, bool ready)
	{
		PandoraDebug.LogInfo("Ready received player : " + hermesPlayerIdx + " is " + ((!ready) ? "NOT" : string.Empty) + " ready", "HERMES");
		foreach (MissionWarbandSave fightingWarband in FightingWarbands)
		{
			if (fightingWarband == null)
			{
				PandoraDebug.LogDebug("a warbandsave is null in FightingWarbands");
			}
			else if (fightingWarband.PlayerIndex == hermesPlayerIdx)
			{
				fightingWarband.IsReady = ready;
				PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_READY);
				return;
			}
		}
		PandoraDebug.LogWarning("Cannot set Ready flag : Player not found among the fighting warbands", "HERMES");
	}

	public void KickPlayerFromLobby(int kickedPlayerIdx)
	{
		Send(true, Hermes.SendTarget.OTHERS, uid, 4u, kickedPlayerIdx);
	}

	public void SendKickPlayer(int kickedPlayerIdx)
	{
		StartCoroutine(SendKickPlayerAsync(kickedPlayerIdx));
	}

	private IEnumerator SendKickPlayerAsync(int kickedPlayerIdx)
	{
		PandoraDebug.LogInfo("SendKickPlayerAsync " + (Hephaestus.LobbyConnexionResult)kickedPlayerIdx, "HERMES");
		PandoraDebug.LogInfo("fc " + PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count + " playertype " + ((PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count <= 1) ? PlayerTypeId.NONE : PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId) + " isconnected " + PandoraSingleton<Hermes>.Instance.IsConnected());
		while (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 && PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.AI && PandoraSingleton<Hermes>.Instance.IsConnected())
		{
			PandoraDebug.LogInfo("SendKickPlayer " + (Hephaestus.LobbyConnexionResult)kickedPlayerIdx, "HERMES");
			Send(true, Hermes.SendTarget.OTHERS, uid, 4u, kickedPlayerIdx);
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void KickPlayerRPC(int kickedPlayerIdx)
	{
		PandoraSingleton<SkirmishManager>.Instance.OnKick((Hephaestus.LobbyConnexionResult)kickedPlayerIdx);
	}

	public void RegisterToHermes()
	{
		PandoraSingleton<Hermes>.Instance.RegisterMyrtilus(this);
		uid = 4294967293u;
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
		if (!locked)
		{
			switch (command)
			{
			case 1u:
			{
				int warbandId2 = (int)parms[0];
				string name2 = (string)parms[1];
				string overrideName2 = (string)parms[2];
				string playerName2 = (string)parms[3];
				int rank2 = (int)parms[4];
				int rating2 = (int)parms[5];
				int playerIndex2 = (int)parms[6];
				int playerTypeId2 = (int)parms[7];
				string[] unitSaves2 = (string[])parms[8];
				AddWarbandInfoRPC(warbandId2, name2, overrideName2, playerName2, rank2, rating2, playerIndex2, playerTypeId2, unitSaves2);
				break;
			}
			case 2u:
			{
				int missionWarbandIdx = (int)parms[0];
				int warbandId = (int)parms[1];
				string name = (string)parms[2];
				string overrideName = (string)parms[3];
				string playerName = (string)parms[4];
				int rank = (int)parms[5];
				int rating = (int)parms[6];
				int playerIndex = (int)parms[7];
				int playerTypeId = (int)parms[8];
				string[] unitSaves = (string[])parms[9];
				SetWarbandInfoRPC(missionWarbandIdx, warbandId, name, overrideName, playerName, rank, rating, playerIndex, playerTypeId, unitSaves);
				break;
			}
			case 3u:
			{
				int seed = (int)parms[0];
				string serializedMission = (string)parms[1];
				bool clear = (bool)parms[2];
				MissionRPC(seed, serializedMission, clear);
				break;
			}
			case 4u:
			{
				int kickedPlayerIdx = (int)parms[0];
				KickPlayerRPC(kickedPlayerIdx);
				break;
			}
			case 5u:
			{
				int hermesPlayerIdx = (int)parms[0];
				bool ready = (bool)parms[1];
				ReadyRPC(hermesPlayerIdx, ready);
				break;
			}
			}
		}
	}
}
