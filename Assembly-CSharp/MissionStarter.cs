using System.Collections.Generic;
using UnityEngine;

public class MissionStarter : MonoBehaviour
{
	public GameObject ground;

	public int seed;

	public DeploymentScenarioMapLayoutId scenarioMapLayoutId;

	public MissionMapLayoutId mapLayoutId;

	public DeploymentScenarioId deployScenarioId;

	public MissionMapGameplayId gameplayId;

	public int turnTimer;

	public int deployTimer;

	public int beaconLimit;

	public WyrdstonePlacementId wyrdPlacementId;

	public WyrdstoneDensityId wyrdDensityId;

	public SearchDensityId searchDensityId;

	public int trapCount;

	public VictoryTypeId victoryTypeId;

	public bool autoDeploy;

	public UnitId roamingUnitId;

	public ProcMissionRatingId missionRating;

	public List<WarbandInitData> warbands;

	private void Awake()
	{
		if (!PandoraSingleton<MissionStartData>.Exists())
		{
			GameObject gameObject = new GameObject("mission_start_data");
			gameObject.AddComponent<MissionStartData>();
		}
	}

	private void Update()
	{
		if (PandoraSingleton<MissionLoader>.Exists())
		{
			PandoraSingleton<MissionStartData>.Instance.ResetSeed();
			MissionSave missionSave = new MissionSave(Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
			missionSave.campaignId = 0;
			missionSave.isCampaign = false;
			missionSave.mapPosition = 0;
			missionSave.rating = 0;
			missionSave.deployScenarioMapLayoutId = (int)scenarioMapLayoutId;
			missionSave.mapGameplayId = (int)gameplayId;
			missionSave.mapLayoutId = (int)mapLayoutId;
			missionSave.deployCount = warbands.Count;
			missionSave.teams = new List<int>();
			missionSave.deployScenarioSlotIds = new List<int>();
			missionSave.VictoryTypeId = (int)victoryTypeId;
			missionSave.objectiveTypeIds = new List<int>();
			missionSave.objectiveTargets = new List<int>();
			missionSave.objectiveSeeds = new List<int>();
			missionSave.turnTimer = turnTimer;
			missionSave.deployTimer = deployTimer;
			missionSave.beaconLimit = beaconLimit;
			missionSave.wyrdPlacementId = (int)wyrdPlacementId;
			missionSave.wyrdDensityId = (int)wyrdDensityId;
			missionSave.searchDensityId = (int)searchDensityId;
			missionSave.autoDeploy = autoDeploy;
			missionSave.roamingUnitId = (int)roamingUnitId;
			missionSave.ratingId = (int)missionRating;
			for (int i = 0; i < warbands.Count; i++)
			{
				missionSave.teams.Add(warbands[i].team);
				missionSave.deployScenarioSlotIds.Add((int)warbands[i].deployId);
				missionSave.objectiveTypeIds.Add((int)warbands[i].objectiveTypeId);
				missionSave.objectiveTargets.Add(warbands[i].objectiveTargetIdx);
				missionSave.objectiveSeeds.Add(PandoraSingleton<GameManager>.Instance.LocalTyche.RandInt());
			}
			PandoraSingleton<MissionStartData>.Instance.SetMission(new Mission(missionSave));
			DataFactory instance = PandoraSingleton<DataFactory>.Instance;
			int num = (int)deployScenarioId;
			List<DeploymentScenarioSlotData> list = instance.InitData<DeploymentScenarioSlotData>("fk_deployment_scenario_id", num.ToString()).ToDynList();
			List<int> list2 = new List<int>();
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Tyche tyche = new Tyche((int)(Random.value * 2.14748365E+09f));
				int index = tyche.Rand(0, list.Count);
				list2.Add((int)list[index].Id);
				list.RemoveAt(index);
			}
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployScenarioSlotIds = list2;
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			if (warbands != null && warbands.Count > 0)
			{
				int num3 = 0;
				foreach (WarbandInitData warband in warbands)
				{
					WarbandSave warbandSave = new WarbandSave(warband.id);
					warbandSave.name = warband.name;
					WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)warband.id);
					foreach (UnitInitData unit in warband.units)
					{
						UnitSave unitSave = new UnitSave(unit.id);
						unitSave.stats.name = unit.name;
						unitSave.rankId = unit.rank;
						foreach (WeaponInitData weapon in unit.weapons)
						{
							unitSave.items[(int)weapon.slotId] = new ItemSave(weapon.itemId, (weapon.qualityId == ItemQualityId.NONE) ? ItemQualityId.NORMAL : weapon.qualityId, weapon.runeMarkId, weapon.runeMarkQualityId, warbandData.AllegianceId);
						}
						foreach (SkillId skill in unit.skills)
						{
							SkillData skillData = PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)skill);
							if (skillData.Passive)
							{
								unitSave.passiveSkills.Add(skillData.Id);
							}
							else
							{
								unitSave.activeSkills.Add(skillData.Id);
							}
						}
						unitSave.spells = unit.spells;
						foreach (MutationId mutation in unit.mutations)
						{
							unitSave.mutations.Add(mutation);
						}
						unitSave.injuries = unit.injuries;
						warbandSave.units.Add(unitSave);
					}
					WarbandMenuController warbandMenuController = new WarbandMenuController(warbandSave);
					PandoraSingleton<MissionStartData>.Instance.AddFightingWarband((WarbandId)warbandMenuController.Warband.GetWarbandSave().id, CampaignWarbandId.NONE, warbandMenuController.Warband.GetWarbandSave().name, warbandMenuController.Warband.GetWarbandSave().overrideName, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_ai"), warband.rank, 0, PandoraSingleton<Hermes>.Instance.PlayerIndex, warband.playerId, warbandMenuController.GetActiveUnitsSerialized());
					list3.Add((int)warband.objectiveTypeId);
					list4.Add(warband.objectiveTargetIdx);
					num3++;
				}
			}
			if (ground != null)
			{
				PandoraSingleton<MissionLoader>.Instance.ground = ground;
				PandoraSingleton<MissionLoader>.Instance.trapCount = trapCount;
			}
			PandoraSingleton<TransitionManager>.Instance.noTransition = true;
			Object.Destroy(base.gameObject);
		}
	}
}
