using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
	public MissionSave missionSave;

	private List<UnitId> roamingUnitIds;

	private static readonly List<UnitData> tempList = new List<UnitData>();

	private static readonly List<Item> tempRemovedItems = new List<Item>();

	public Mission(MissionSave missionSave)
	{
		this.missionSave = missionSave;
	}

	public override string ToString()
	{
		return " deploymentScenarioMapLayout = " + (DeploymentScenarioMapLayoutId)missionSave.deployScenarioMapLayoutId + " mapLayout = " + (MissionMapLayoutId)missionSave.mapLayoutId + " deployCount = " + missionSave.deployCount + " wyrdPlacement = " + (WyrdstonePlacementId)missionSave.wyrdPlacementId + " wyrdDensity = " + (WyrdstoneDensityId)missionSave.wyrdDensityId;
	}

	public void RefreshDifficulty(int rating, bool isProc)
	{
		int num = 0;
		int num2 = 0;
		List<ProcMissionRatingData> list = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>();
		for (int i = 0; i < list.Count; i++)
		{
			num2 = Mathf.Max(num2, list[i].MaxValue);
		}
		if (!isProc)
		{
			num = (int)(((float)missionSave.rating / (float)rating - 1f) * 100f);
		}
		else
		{
			if (missionSave.ratingId != 0)
			{
				return;
			}
			if (missionSave.rating < 0)
			{
				missionSave.ratingId = -rating;
				return;
			}
			num = ((rating >= 100) ? ((int)(((float)missionSave.rating / (float)rating - 1f) * 100f)) : rating);
		}
		num = Mathf.Clamp(num, 0, num2);
		ProcMissionRatingId id = PandoraSingleton<DataFactory>.Instance.InitDataClosest<ProcMissionRatingData>("max_value", num, lower: false).Id;
		id = ((id == ProcMissionRatingId.NONE) ? ProcMissionRatingId.NORMAL : id);
		missionSave.ratingId = (int)id;
	}

	public DistrictId GetDistrictId()
	{
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
		MissionMapData missionMapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deploymentScenarioMapLayoutData.MissionMapId);
		return missionMapData.DistrictId;
	}

	public MissionMapId GetMapId()
	{
		if (missionSave.randomMap)
		{
			return MissionMapId.NONE;
		}
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
		return deploymentScenarioMapLayoutData.MissionMapId;
	}

	public MissionMapLayoutId GetMapLayoutId()
	{
		if (missionSave.randomLayout)
		{
			return MissionMapLayoutId.NONE;
		}
		return (MissionMapLayoutId)missionSave.mapLayoutId;
	}

	public MissionMapGameplayId GetMapGameplayId()
	{
		if (missionSave.randomGameplay)
		{
			return MissionMapGameplayId.NONE;
		}
		return (MissionMapGameplayId)missionSave.mapGameplayId;
	}

	public string GetSkyName()
	{
		if (missionSave.randomLayout)
		{
			return "random";
		}
		MissionMapLayoutData missionMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapLayoutData>((int)GetMapLayoutId());
		return missionMapLayoutData.Name;
	}

	public bool IsAmbush()
	{
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
		return deploymentScenarioMapLayoutData.Ambush;
	}

	public DeploymentScenarioId GetDeploymentScenarioId()
	{
		if (missionSave.randomDeployment)
		{
			return DeploymentScenarioId.NONE;
		}
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
		return deploymentScenarioMapLayoutData.DeploymentScenarioId;
	}

	public DeploymentId GetDeploymentId(int idx)
	{
		if (missionSave.randomSlots)
		{
			return DeploymentId.NONE;
		}
		DeploymentScenarioSlotData deploymentScenarioSlotData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>(missionSave.deployScenarioSlotIds[idx]);
		return deploymentScenarioSlotData.DeploymentId;
	}

	public List<DeploymentScenarioSlotId> GetMissionDeploySlots()
	{
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(missionSave.deployScenarioMapLayoutId);
		List<DeploymentScenarioSlotData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>("fk_deployment_scenario_id", ((int)deploymentScenarioMapLayoutData.DeploymentScenarioId).ToString());
		List<DeploymentScenarioSlotId> list2 = new List<DeploymentScenarioSlotId>();
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i].Id);
		}
		return list2;
	}

	public void SetRandomDeploySlots(Tyche tyche)
	{
		missionSave.randomSlots = true;
		List<DeploymentScenarioSlotId> missionDeploySlots = PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetMissionDeploySlots();
		for (int i = 0; i < missionSave.deployCount; i++)
		{
			int index = tyche.Rand(0, missionDeploySlots.Count);
			missionSave.deployScenarioSlotIds[i] = (int)missionDeploySlots[index];
			missionDeploySlots.RemoveAt(index);
		}
	}

	public void SetDeploySlots(int pos, int idx)
	{
		missionSave.randomSlots = false;
		List<DeploymentScenarioSlotId> missionDeploySlots = PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetMissionDeploySlots();
		missionSave.deployScenarioSlotIds[pos] = (int)missionDeploySlots[idx];
		idx = (idx + 1) % missionDeploySlots.Count;
		missionSave.deployScenarioSlotIds[(pos + 1) % missionSave.deployCount] = (int)missionDeploySlots[idx];
	}

	public bool HasObjectives()
	{
		return missionSave.objectiveTypeIds != null && missionSave.objectiveTypeIds.Count > 0 && missionSave.objectiveTypeIds[0] != 0;
	}

	public void ClearObjectives()
	{
		missionSave.VictoryTypeId = 1;
		for (int i = 0; i < missionSave.deployCount; i++)
		{
			missionSave.objectiveTypeIds[i] = 0;
			missionSave.randomObjectives[i] = false;
		}
	}

	public void RandomizeObjectives(Tyche tyche)
	{
		for (int i = 0; i < missionSave.deployCount; i++)
		{
			SetRandomObjective(tyche, i);
		}
	}

	public void SetObjective(int pos, int idx)
	{
		missionSave.randomObjectives[pos] = false;
		List<PrimaryObjectiveTypeId> availableObjectiveTypes = GetAvailableObjectiveTypes(pos);
		missionSave.objectiveTypeIds[pos] = (int)availableObjectiveTypes[idx];
	}

	public void SetRandomObjective(Tyche tyche, int pos)
	{
		missionSave.randomObjectives[pos] = true;
		List<PrimaryObjectiveTypeId> availableObjectiveTypes = GetAvailableObjectiveTypes(pos);
		int index = tyche.Rand(0, availableObjectiveTypes.Count);
		missionSave.objectiveTypeIds[pos] = (int)availableObjectiveTypes[index];
	}

	public List<PrimaryObjectiveTypeId> GetAvailableObjectiveTypes(int idx)
	{
		int id = missionSave.deployScenarioSlotIds[missionSave.objectiveTargets[idx]];
		DeploymentScenarioSlotData deploymentScenarioSlotData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>(id);
		List<DeploymentJoinPrimaryObjectiveTypeData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentJoinPrimaryObjectiveTypeData>("fk_deployment_id", ((int)deploymentScenarioSlotData.DeploymentId).ToString());
		return list.ConvertAll((DeploymentJoinPrimaryObjectiveTypeData x) => x.PrimaryObjectiveTypeId);
	}

	public bool IsObjectiveRandom(int idx)
	{
		return missionSave.randomObjectives[idx];
	}

	public PrimaryObjectiveTypeId GetObjectiveTypeId(int idx)
	{
		return (PrimaryObjectiveTypeId)missionSave.objectiveTypeIds[idx];
	}

	public void SetRandomRoaming(Tyche tyche)
	{
		List<UnitRoamingData> datas = PandoraSingleton<DataFactory>.Instance.InitData<UnitRoamingData>();
		UnitRoamingData randomRatio = UnitRoamingData.GetRandomRatio(datas, tyche);
		missionSave.randomRoaming = true;
		missionSave.roamingUnitId = (int)randomRatio.UnitId;
	}

	public List<UnitId> GetRoamingUnitIds()
	{
		if (roamingUnitIds == null)
		{
			List<UnitRoamingData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitRoamingData>();
			roamingUnitIds = list.ConvertAll((UnitRoamingData x) => x.UnitId);
		}
		return roamingUnitIds;
	}

	public void SetRoamingUnit(int idx)
	{
		missionSave.randomRoaming = false;
		missionSave.roamingUnitId = (int)GetRoamingUnitIds()[idx];
	}

	public static Mission GenerateSkirmishMission(MissionMapId mapId = MissionMapId.NONE, DeploymentScenarioId scenarioId = DeploymentScenarioId.NONE)
	{
		Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		List<DeploymentScenarioMapLayoutData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>("skirmish", "1").ToDynList();
		if (mapId != 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].MissionMapId != mapId)
				{
					list.RemoveAt(num);
				}
				else if (scenarioId != 0 && list[num].DeploymentScenarioId != scenarioId)
				{
					list.RemoveAt(num);
				}
			}
		}
		int index = localTyche.Rand(0, list.Count);
		DeploymentScenarioMapLayoutData scenarioMapData = list[index];
		MissionSave procMissionSave = GetProcMissionSave(localTyche, scenarioMapData);
		procMissionSave.isSkirmish = true;
		procMissionSave.randomRoaming = false;
		procMissionSave.roamingUnitId = 0;
		procMissionSave.rating = 9001;
		procMissionSave.randomMap = (mapId == MissionMapId.NONE);
		procMissionSave.randomLayout = true;
		procMissionSave.randomDeployment = (scenarioId == DeploymentScenarioId.NONE);
		procMissionSave.randomSlots = true;
		return new Mission(procMissionSave);
	}

	public static Mission GenerateProcMission(ref List<KeyValuePair<int, int>> mapPositions, Dictionary<int, int>[] wyrdstoneDensityModifiers, Dictionary<int, int>[] searchDensityModifiers, Dictionary<ProcMissionRatingId, int> missionRatingModifiers)
	{
		Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		List<DeploymentScenarioMapLayoutData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>("procedural", "1");
		int index = localTyche.Rand(0, list.Count);
		DeploymentScenarioMapLayoutData scenarioMapData = list[index];
		MissionSave procMissionSave = GetProcMissionSave(localTyche, scenarioMapData);
		List<ProcMissionRatingData> datas = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>();
		ProcMissionRatingData randomRatio = ProcMissionRatingData.GetRandomRatio(datas, localTyche, missionRatingModifiers);
		procMissionSave.ratingId = (int)randomRatio.Id;
		List<ProcMissionRatingWyrdstoneDensityData> datas2 = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingWyrdstoneDensityData>("fk_proc_mission_rating_id", randomRatio.Id.ToIntString());
		ProcMissionRatingWyrdstoneDensityData randomRatio2 = ProcMissionRatingWyrdstoneDensityData.GetRandomRatio(datas2, localTyche, wyrdstoneDensityModifiers[(int)randomRatio.Id]);
		procMissionSave.wyrdDensityId = (int)randomRatio2.WyrdstoneDensityId;
		List<ProcMissionRatingSearchDensityData> datas3 = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingSearchDensityData>("fk_proc_mission_rating_id", randomRatio.Id.ToIntString());
		ProcMissionRatingSearchDensityData randomRatio3 = ProcMissionRatingSearchDensityData.GetRandomRatio(datas3, localTyche, searchDensityModifiers[(int)randomRatio.Id]);
		procMissionSave.searchDensityId = (int)randomRatio3.SearchDensityId;
		Mission mission = new Mission(procMissionSave);
		DistrictData districtData = PandoraSingleton<DataFactory>.Instance.InitData<DistrictData>((int)mission.GetDistrictId());
		bool flag = true;
		int num = 0;
		int num2 = 0;
		while (flag && num < 100)
		{
			flag = false;
			num2 = localTyche.Rand(0, districtData.Slots);
			for (int i = 0; i < mapPositions.Count; i++)
			{
				if (mapPositions[i].Key == (int)mission.GetDistrictId() && mapPositions[i].Value == num2)
				{
					flag = true;
					break;
				}
			}
			num++;
		}
		procMissionSave.mapPosition = num2;
		mapPositions.Add(new KeyValuePair<int, int>((int)mission.GetDistrictId(), num2));
		mission.RandomizeObjectives(localTyche);
		return mission;
	}

	public static MissionSave GetProcMissionSave(Tyche tyche, DeploymentScenarioMapLayoutData scenarioMapData)
	{
		DeploymentScenarioData deploymentScenarioData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioData>((int)scenarioMapData.DeploymentScenarioId);
		List<DeploymentScenarioSlotData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>("fk_deployment_scenario_id", ((int)deploymentScenarioData.Id).ToConstantString()).ToDynList();
		List<int> list2 = new List<int>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int index = tyche.Rand(0, list.Count);
			list2.Add((int)list[index].Id);
			list.RemoveAt(index);
		}
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		List<int> list6 = new List<int>();
		List<bool> list7 = new List<bool>();
		for (int i = 0; i < list2.Count; i++)
		{
			list3.Add(i);
			list4.Add(0);
			list7.Add(item: false);
			list6.Add(PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, int.MaxValue));
			list5.Add((i + 1) % list2.Count);
		}
		MissionMapId missionMapId = scenarioMapData.MissionMapId;
		MissionMapLayoutId missionMapLayoutId = scenarioMapData.MissionMapLayoutId;
		if (missionMapLayoutId == MissionMapLayoutId.NONE)
		{
			DataFactory instance = PandoraSingleton<DataFactory>.Instance;
			int num2 = (int)missionMapId;
			List<MissionMapLayoutData> list8 = instance.InitData<MissionMapLayoutData>("fk_mission_map_id", num2.ToString());
			int index2 = tyche.Rand(0, list8.Count);
			missionMapLayoutId = list8[index2].Id;
		}
		MissionMapGameplayId mapGameplayId = MissionMapGameplayId.NONE;
		DataFactory instance2 = PandoraSingleton<DataFactory>.Instance;
		int num3 = (int)missionMapId;
		List<MissionMapGameplayData> list9 = instance2.InitData<MissionMapGameplayData>("fk_mission_map_id", num3.ToString());
		if (list9.Count > 0)
		{
			int index3 = tyche.Rand(0, list9.Count);
			mapGameplayId = list9[index3].Id;
		}
		List<WyrdstonePlacementData> datas = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstonePlacementData>();
		WyrdstonePlacementData randomRatio = WyrdstonePlacementData.GetRandomRatio(datas, tyche);
		List<WyrdstoneDensityData> datas2 = PandoraSingleton<DataFactory>.Instance.InitData<WyrdstoneDensityData>();
		WyrdstoneDensityData randomRatio2 = WyrdstoneDensityData.GetRandomRatio(datas2, tyche);
		List<SearchDensityData> datas3 = PandoraSingleton<DataFactory>.Instance.InitData<SearchDensityData>();
		SearchDensityData randomRatio3 = SearchDensityData.GetRandomRatio(datas3, tyche);
		List<UnitRoamingData> datas4 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRoamingData>();
		UnitRoamingData randomRatio4 = UnitRoamingData.GetRandomRatio(datas4, tyche);
		MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
		missionSave.campaignId = 0;
		missionSave.isCampaign = false;
		missionSave.deployScenarioMapLayoutId = (int)scenarioMapData.Id;
		missionSave.mapLayoutId = (int)missionMapLayoutId;
		missionSave.mapGameplayId = (int)mapGameplayId;
		missionSave.randomGameplay = (list9.Count > 0);
		missionSave.deployCount = list2.Count;
		missionSave.teams = list3;
		missionSave.deployScenarioSlotIds = list2;
		missionSave.VictoryTypeId = 1;
		missionSave.objectiveTypeIds = list4;
		missionSave.objectiveTargets = list5;
		missionSave.objectiveSeeds = list6;
		missionSave.randomObjectives = list7;
		missionSave.wyrdPlacementId = (int)randomRatio.Id;
		missionSave.wyrdDensityId = (int)randomRatio2.Id;
		missionSave.searchDensityId = (int)randomRatio3.Id;
		missionSave.randomRoaming = true;
		missionSave.roamingUnitId = (int)(randomRatio4?.UnitId ?? UnitId.NONE);
		return missionSave;
	}

	public static Mission GenerateAmbushMission(Mission refMission)
	{
		Tyche localTyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		List<DeploymentScenarioMapLayoutData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>("ambush", "1");
		int index = localTyche.Rand(0, list.Count);
		DeploymentScenarioMapLayoutData scenarioMapData = list[index];
		MissionSave procMissionSave = GetProcMissionSave(localTyche, scenarioMapData);
		procMissionSave.rating = refMission.missionSave.rating;
		procMissionSave.ratingId = refMission.missionSave.ratingId;
		Mission mission = new Mission(procMissionSave);
		mission.RandomizeObjectives(localTyche);
		mission.missionSave.VictoryTypeId = refMission.missionSave.VictoryTypeId;
		mission.missionSave.wyrdPlacementId = refMission.missionSave.wyrdPlacementId;
		mission.missionSave.wyrdDensityId = refMission.missionSave.wyrdDensityId;
		mission.missionSave.searchDensityId = refMission.missionSave.searchDensityId;
		mission.missionSave.ratingId = refMission.missionSave.ratingId;
		return mission;
	}

	public static IEnumerator GetProcWarband(int rating, int warRank, int warUnitsCount, bool impressive, WarbandData warData, int heroesCount, int highestUnitRank, Action<WarbandSave> callback)
	{
		Tyche tyche = PandoraSingleton<GameManager>.Instance.LocalTyche;
		int ratingPool = 0;
		PandoraDebug.LogInfo("Generating Procedural Warband using a rating of : " + rating, "MISSION");
		WarbandSave warSave = new WarbandSave(warData.Id);
		List<WarbandNameData> warNamesData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandNameData>("fk_warband_id", ((int)warData.Id).ToString());
		warSave.name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(warNamesData[tyche.Rand(0, warNamesData.Count)].Name);
		warSave.rank = warRank;
		List<ProcWarbandRankData> procWarRanksData = PandoraSingleton<DataFactory>.Instance.InitData<ProcWarbandRankData>("fk_warband_rank_id", warRank.ToString());
		ProcWarbandRankData procWarRankData = null;
		procWarRankData = procWarRanksData[0];
		if (procWarRankData.Id == ProcWarbandRankId.NONE)
		{
			procWarRankData = procWarRanksData[1];
		}
		string procWarbandRank = ((int)procWarRankData.Id).ToString();
		List<ColorPresetData> presetsData = PandoraSingleton<DataFactory>.Instance.InitData<ColorPresetData>("fk_warband_id", ((int)warData.Id).ToString());
		ColorPresetData colorPresetData = presetsData[tyche.Rand(0, presetsData.Count)];
		int offsetPreset = (int)colorPresetData.Id << 8;
		List<AttributeData> attributeDataList = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>();
		List<ProcWarbandRankJoinUnitTypeData> unitTypesData = PandoraSingleton<DataFactory>.Instance.InitData<ProcWarbandRankJoinUnitTypeData>("fk_proc_warband_rank_id", procWarbandRank);
		List<HireUnitInjuryData> injuriesData = PandoraSingleton<DataFactory>.Instance.InitData<HireUnitInjuryData>("unit_rank", highestUnitRank.ToString());
		unitTypesData.Sort(new ProcWarbandRankJoinUnitTypeDataSorter());
		List<UnitRating> unitRatings = new List<UnitRating>();
		List<UnitData> warbandUnitsData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>("fk_warband_id", ((int)warData.Id).ToString(), "released", "1");
		UnitRating unitRating4 = new UnitRating();
		unitRatings.Add(unitRating4);
		yield return PandoraSingleton<GameManager>.Instance.StartCoroutine(AddProcUnit(unitRating4, tyche, warbandUnitsData, UnitTypeId.LEADER, offsetPreset, injuriesData));
		ratingPool += unitRating4.rating;
		List<UnitTypeId> heroesTypes = new List<UnitTypeId>();
		for (int i3 = 0; i3 < unitTypesData.Count; i3++)
		{
			switch (unitTypesData[i3].UnitTypeId)
			{
			case UnitTypeId.HERO_1:
			case UnitTypeId.HERO_2:
			case UnitTypeId.HERO_3:
			{
				for (int i = 0; i < unitTypesData[i3].MaxCount; i++)
				{
					heroesTypes.Add(unitTypesData[i3].UnitTypeId);
				}
				break;
			}
			}
		}
		if (impressive)
		{
			unitRating4 = new UnitRating();
			unitRatings.Add(unitRating4);
			yield return PandoraSingleton<GameManager>.Instance.StartCoroutine(AddProcUnit(unitRating4, tyche, warbandUnitsData, UnitTypeId.IMPRESSIVE, offsetPreset, injuriesData));
			ratingPool += unitRating4.rating;
		}
		for (int i2 = 0; i2 < heroesCount; i2++)
		{
			unitRating4 = new UnitRating();
			unitRatings.Add(unitRating4);
			yield return PandoraSingleton<GameManager>.Instance.StartCoroutine(AddProcUnit(unitRating4, tyche, warbandUnitsData, heroesTypes, offsetPreset, injuriesData));
			ratingPool += unitRating4.rating;
		}
		while (unitRatings.Count < warUnitsCount)
		{
			unitRating4 = new UnitRating();
			unitRatings.Add(unitRating4);
			yield return PandoraSingleton<GameManager>.Instance.StartCoroutine(AddProcUnit(unitRating4, tyche, warbandUnitsData, UnitTypeId.HENCHMEN, offsetPreset, injuriesData));
			ratingPool += unitRating4.rating;
		}
		for (int k2 = 0; k2 < unitRatings.Count; k2++)
		{
			if (ratingPool >= rating)
			{
				break;
			}
			Unit unit = unitRatings[k2].unit;
			int garbageRating = 0;
			UnitFactory.AddArmorStyleSet(tyche, ref garbageRating, unit);
			int newRating = unit.GetRating();
			ratingPool += newRating - unitRatings[k2].rating;
			unitRatings[k2].rating = newRating;
		}
		bool hasChanges2 = true;
		while (ratingPool < rating && hasChanges2)
		{
			hasChanges2 = false;
			for (int n = 0; n < unitRatings.Count; n++)
			{
				if (ratingPool >= rating)
				{
					break;
				}
				hasChanges2 |= AdvanceUnit(tyche, unitRatings[n], highestUnitRank, attributeDataList, rating, ref ratingPool);
			}
			yield return null;
		}
		ItemQualityId maxQualityId = ItemQualityId.NORMAL;
		if (ratingPool < rating)
		{
			List<ProcWarbandRankJoinItemQualityData> procItemQualitiesData = PandoraSingleton<DataFactory>.Instance.InitData<ProcWarbandRankJoinItemQualityData>("fk_proc_warband_rank_id", procWarbandRank);
			for (int p = 0; p < procItemQualitiesData.Count; p++)
			{
				ProcWarbandRankJoinItemQualityData warRankItemQualityData = procItemQualitiesData[p];
				maxQualityId = ((warRankItemQualityData.ItemQualityId <= maxQualityId) ? maxQualityId : warRankItemQualityData.ItemQualityId);
				int counter = tyche.Rand(warRankItemQualityData.MinCount, warRankItemQualityData.MaxCount + 1);
				for (int m = 0; m < unitRatings.Count; m++)
				{
					UnitFactory.BoostItemsQuality(tyche, unitRatings[m].unit, warRankItemQualityData.ItemQualityId, ref ratingPool, ref counter, rating);
				}
			}
		}
		bool added = true;
		while (added)
		{
			int j = 0;
			while (added && j < unitRatings.Count)
			{
				added = false;
				if (ratingPool < rating && !unitRatings[j].unit.IsInventoryFull())
				{
					Item cons = UnitFactory.GetProcItem(tyche, ref ratingPool, unitRatings[j].unit, UnitSlotId.ITEM_1, (tyche.Rand(0, 2) != 0) ? ItemTypeId.CONSUMABLE_POTIONS : ItemTypeId.CONSUMABLE_MISC, maxQualityId);
					ratingPool += cons.GetRating();
					unitRatings[j].unit.GetEmptyItemSlot(out UnitSlotId slotId, cons);
					unitRatings[j].unit.EquipItem(slotId, cons);
					added = true;
				}
				j++;
			}
		}
		hasChanges2 = true;
		while (ratingPool < rating && hasChanges2)
		{
			hasChanges2 = false;
			for (int l = 0; l < unitRatings.Count; l++)
			{
				if (ratingPool >= rating)
				{
					break;
				}
				hasChanges2 |= AdvanceUnit(maxRank: GetWarbandRankUnitType(unitTypesData, unitRatings[l].unit.GetUnitTypeId()).MaxRank, tyche: tyche, unitRating: unitRatings[l], attributeDataList: attributeDataList, ratingCheck: rating, ratingPool: ref ratingPool);
			}
			yield return null;
		}
		for (int k = 0; k < unitRatings.Count; k++)
		{
			warSave.units.Add(unitRatings[k].unit.UnitSave);
		}
		callback?.Invoke(warSave);
	}

	public static bool AdvanceUnit(Tyche tyche, UnitRating unitRating, int maxRank, List<AttributeData> attributeDataList, int ratingCheck, ref int ratingPool, bool full = false)
	{
		bool result = false;
		List<UnitJoinUnitRankData> list = new List<UnitJoinUnitRankData>();
		List<Mutation> list2 = new List<Mutation>();
		List<Item> previousItems = new List<Item>();
		Unit unit = unitRating.unit;
		if (unit.Rank < maxRank)
		{
			unit.AddXp(99999, list, list2, previousItems, 0, (!full) ? (unit.Rank + 1) : maxRank);
			if (list.Count > 0)
			{
				result = true;
			}
			if (list2.Count > 0)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					if (list2[i].GroupData.UnitSlotId == UnitSlotId.SET1_MAINHAND || list2[i].GroupData.UnitSlotId == UnitSlotId.SET1_OFFHAND)
					{
						GenerateUnitWeapons(tyche, unit);
					}
				}
			}
			int ratingPool2 = 0;
			UnitFactory.RaiseAttributes(tyche, attributeDataList, unit, ref ratingPool2, ratingCheck, unitRating.baseAttributes, unitRating.maxAttributes);
			unitRating.UpdateBaseAttributes();
			UnitFactory.AddSkillSpells(tyche, unit, ref ratingPool2, ratingCheck, unitRating.skillsData, unitRating.baseAttributes);
			unitRating.UpdateBaseAttributes();
			int rating = unit.GetRating();
			ratingPool += rating - unitRating.rating;
			unitRating.rating = rating;
		}
		return result;
	}

	private static ProcWarbandRankJoinUnitTypeData GetWarbandRankUnitType(List<ProcWarbandRankJoinUnitTypeData> warbandRankJoinUnitTypeDatas, UnitTypeId unitTypeId)
	{
		for (int i = 0; i < warbandRankJoinUnitTypeDatas.Count; i++)
		{
			if (warbandRankJoinUnitTypeDatas[i].UnitTypeId == unitTypeId)
			{
				return warbandRankJoinUnitTypeDatas[i];
			}
		}
		return null;
	}

	private static IEnumerator AddProcUnit(UnitRating unitRating, Tyche tyche, List<UnitData> warbandUnitsData, List<UnitTypeId> unitTypeId, int offsetPreset, List<HireUnitInjuryData> injuriesData)
	{
		int index = tyche.Rand(0, unitTypeId.Count);
		UnitTypeId typeId = unitTypeId[index];
		unitTypeId.RemoveAt(index);
		yield return AddProcUnit(unitRating, tyche, warbandUnitsData, typeId, offsetPreset, injuriesData);
	}

	public static IEnumerator AddProcUnit(UnitRating unitRating, Tyche tyche, List<UnitData> warbandUnitsData, UnitTypeId unitTypeId, int offsetPreset, List<HireUnitInjuryData> injuriesData)
	{
		tempList.Clear();
		for (int i = 0; i < warbandUnitsData.Count; i++)
		{
			if (warbandUnitsData[i].UnitTypeId == unitTypeId)
			{
				tempList.Add(warbandUnitsData[i]);
			}
		}
		UnitData unitData = (tempList.Count != 1) ? tempList[tyche.Rand(0, tempList.Count)] : tempList[0];
		Unit unit = new Unit(new UnitSave(unitData.Id));
		yield return null;
		List<InjuryId> excludesInjuryIds = new List<InjuryId>(Unit.HIRE_UNIT_INJURY_EXCLUDES);
		HireUnitInjuryData injuryData = HireUnitInjuryData.GetRandomRatio(injuriesData, tyche);
		for (int j = 0; j < injuryData.Count; j++)
		{
			InjuryData newInjury = PandoraSingleton<HideoutManager>.Instance.Progressor.RollInjury(excludesInjuryIds, unit);
			if (newInjury != null)
			{
				unit.AddInjury(newInjury, PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate, tempRemovedItems, isHireUnit: true);
				tempRemovedItems.Clear();
				excludesInjuryIds.Add(newInjury.Id);
				continue;
			}
			break;
		}
		foreach (BodyPartId bodyPartId in unit.bodyParts.Keys)
		{
			KeyValuePair<int, int> kv = unit.UnitSave.customParts[bodyPartId];
			unit.UnitSave.customParts[bodyPartId] = new KeyValuePair<int, int>(kv.Key, offsetPreset);
		}
		tempList.Clear();
		GenerateUnitWeapons(tyche, unit);
		unitRating.unit = unit;
		unitRating.rating = unit.GetRating();
		unitRating.skillsData = UnitFactory.GetLearnableSkills(unit);
		unitRating.UpdateMaxAttributes();
		unitRating.UpdateBaseAttributes();
	}

	private static void GenerateUnitWeapons(Tyche tyche, Unit unit)
	{
		int ratingPool = 0;
		CombatStyleId excludedCombatStyleId = UnitFactory.AddCombatStyleSet(tyche, ref ratingPool, unit, UnitSlotId.SET1_MAINHAND);
		UnitFactory.AddCombatStyleSet(tyche, ref ratingPool, unit, UnitSlotId.SET2_MAINHAND, excludedCombatStyleId);
	}

	public static Mission GenerateCampaignMission(WarbandId warbandId, int index)
	{
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		string[] fields = new string[2]
		{
			"fk_warband_id",
			"idx"
		};
		string[] array = new string[2];
		int num = (int)warbandId;
		array[0] = num.ToString();
		array[1] = index.ToString();
		List<CampaignMissionData> list = instance.InitData<CampaignMissionData>(fields, array);
		if (list.Count == 0)
		{
			return null;
		}
		CampaignMissionData campaignMissionData = list[0];
		List<DeploymentScenarioMapLayoutData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>("fk_deployment_scenario_id", ((int)campaignMissionData.DeploymentScenarioId).ToString());
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = list2[0];
		List<CampaignMissionJoinCampaignWarbandData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionJoinCampaignWarbandData>("fk_campaign_mission_id", ((int)campaignMissionData.Id).ToString());
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		List<int> list6 = new List<int>();
		List<int> list7 = new List<int>();
		List<bool> list8 = new List<bool>();
		List<int> list9 = new List<int>();
		for (int i = 0; i < list3.Count; i++)
		{
			list5.Add((list3[i].CampaignWarbandId != 0) ? 25 : 24);
			list4.Add(list3[i].Team);
			list6.Add(0);
			list7.Add(0);
			list8.Add(item: false);
			list9.Add(0);
		}
		MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
		missionSave.mapPosition = campaignMissionData.MapPos;
		missionSave.rating = campaignMissionData.Rating;
		missionSave.campaignId = (int)campaignMissionData.Id;
		missionSave.isCampaign = true;
		missionSave.isTuto = campaignMissionData.IsTuto;
		missionSave.deployScenarioMapLayoutId = (int)deploymentScenarioMapLayoutData.Id;
		missionSave.mapLayoutId = (int)deploymentScenarioMapLayoutData.MissionMapLayoutId;
		missionSave.deployCount = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionJoinCampaignWarbandData>("fk_campaign_mission_id", campaignMissionData.Id.ToIntString()).Count;
		missionSave.teams = list4;
		missionSave.deployScenarioSlotIds = list5;
		missionSave.VictoryTypeId = (missionSave.isTuto ? 1 : 2);
		missionSave.objectiveTypeIds = list6;
		missionSave.objectiveTargets = list7;
		missionSave.objectiveSeeds = list9;
		missionSave.randomObjectives = list8;
		missionSave.wyrdPlacementId = (int)campaignMissionData.WyrdstonePlacementId;
		missionSave.wyrdDensityId = (int)campaignMissionData.WyrdstoneDensityId;
		missionSave.searchDensityId = (int)campaignMissionData.SearchDensityId;
		return new Mission(missionSave);
	}

	public static WarbandSave GetCampaignWarband(CampaignMissionJoinCampaignWarbandData warbandData)
	{
		List<AttributeData> attributesData = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>();
		CampaignWarbandData campaignWarbandData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandData>((int)warbandData.CampaignWarbandId);
		WarbandSave warbandSave = new WarbandSave(campaignWarbandData.WarbandId);
		warbandSave.campaignId = (int)warbandData.CampaignWarbandId;
		warbandSave.rank = campaignWarbandData.Rank;
		List<CampaignWarbandJoinCampaignUnitData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandJoinCampaignUnitData>("fk_campaign_warband_id", ((int)warbandData.CampaignWarbandId).ToString());
		List<UnitSave> list2 = new List<UnitSave>();
		for (int i = 0; i < list.Count; i++)
		{
			CampaignUnitData campaignUnitData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitData>((int)list[i].CampaignUnitId);
			UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>((int)campaignUnitData.UnitId);
			UnitSave unitSave = new UnitSave(campaignUnitData.UnitId);
			unitSave.campaignId = (int)campaignUnitData.Id;
			if (!string.IsNullOrEmpty(campaignUnitData.FirstName))
			{
				unitSave.stats.name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(campaignUnitData.FirstName);
			}
			else if (!string.IsNullOrEmpty(unitData.FirstName))
			{
				unitSave.stats.name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(unitData.FirstName);
			}
			List<UnitRankData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>("rank", campaignUnitData.Rank.ToString());
			unitSave.rankId = list3[0].Id;
			List<CampaignUnitJoinSkillData> list4 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitJoinSkillData>("fk_campaign_unit_id", campaignUnitData.Id.ToIntString());
			for (int j = 0; j < list4.Count; j++)
			{
				SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)list4[j].SkillId);
				if (skillData.SkillTypeId == SkillTypeId.SKILL_ACTION)
				{
					if (skillData.Passive)
					{
						unitSave.passiveSkills.Add(skillData.Id);
					}
					else
					{
						unitSave.activeSkills.Add(skillData.Id);
					}
				}
				else if (skillData.SkillTypeId == SkillTypeId.SPELL_ACTION)
				{
					unitSave.spells.Add(skillData.Id);
				}
			}
			List<CampaignUnitJoinMutationData> list5 = PandoraSingleton<DataFactory>.Instance.InitData<CampaignUnitJoinMutationData>("fk_campaign_unit_id", campaignUnitData.Id.ToIntString());
			for (int k = 0; k < list5.Count; k++)
			{
				unitSave.mutations.Add(list5[k].MutationId);
			}
			Unit unit = new Unit(unitSave);
			int ratingPool = 0;
			int ratingMax = 999999;
			UnitFactory.RaiseAttributes(PandoraSingleton<GameManager>.Instance.LocalTyche, attributesData, unit, ref ratingPool, ratingMax);
			UnitFactory.AddSkillSpells(PandoraSingleton<GameManager>.Instance.LocalTyche, unit, ref ratingPool, ratingMax);
			list2.Add(unitSave);
		}
		warbandSave.units = list2;
		return warbandSave;
	}
}
