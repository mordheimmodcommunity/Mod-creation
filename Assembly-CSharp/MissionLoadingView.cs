using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionLoadingView : LoadingView
{
	public MissionHeaderView missionHeaderView;

	public GameObject campaignDescription;

	public GameObject objectivesView;

	public Text deploymentTitle;

	public Text deploymentDesc;

	public Text descriptionTitle;

	public Text descriptionDesc;

	public GameObject ambush;

	public Image ambusher;

	public Image ambushed;

	public Sprite bulletPoint;

	public GameObject skirmishView;

	public Text warbandName;

	public Text warbandRank;

	public Text warbandRating;

	public Image warbandFlag;

	public Text opponentWarbandName;

	public Text opponentWarbandRank;

	public Text opponentWarbandRating;

	public Image opponentWarbandFlag;

	public Text gameplayType;

	public Text gameplayTypeDesc;

	public Text deploymentType;

	public Text deploymentTypeDesc;

	public Text optionalObjectivePlayerName;

	public Text optionalObjective;

	public Text optionalObjectiveOpponentName;

	public Text optionalObjectiveOpponent;

	private Mission mission;

	private CampaignMissionData missionData;

	public override void Show()
	{
		base.Show();
		mission = PandoraSingleton<MissionStartData>.Instance.CurrentMission;
		missionData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(mission.missionSave.campaignId);
		missionHeaderView.Init(mission, missionData);
		int num = 0;
		for (int i = 0; i < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count; i++)
		{
			if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands[i].PlayerTypeId == PlayerTypeId.PLAYER)
			{
				num++;
			}
		}
		if (mission.missionSave.isTuto)
		{
			OnTutorial();
		}
		else if (mission.missionSave.isCampaign)
		{
			OnCampaign();
		}
		else if (mission.missionSave.isSkirmish || num > 1)
		{
			OnSkirmish();
		}
		else
		{
			OnMission();
		}
	}

	public void OnCampaign()
	{
		missionHeaderView.OnCampaign();
		List<Objective> objectives = new List<Objective>();
		UIObjectivesController component = objectivesView.GetComponent<UIObjectivesController>();
		component.SetObjectives(objectives, loading: true);
		objectivesView.gameObject.SetActive(value: false);
		((Component)(object)deploymentTitle).gameObject.SetActive(value: false);
		((Component)(object)deploymentDesc).gameObject.SetActive(value: false);
		ambush.SetActive(value: false);
		((Component)(object)ambusher).gameObject.SetActive(value: false);
		((Component)(object)ambushed).gameObject.SetActive(value: false);
		skirmishView.SetActive(value: false);
		descriptionTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_title_" + missionData.Name));
		descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_prologue_" + missionData.Name));
		LoadBackground("bg_" + missionData.Name + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionData.LoadingImageCount));
		LoadDialog("mission_camp_prologue_" + missionData.Name);
	}

	public void OnTutorial()
	{
		missionHeaderView.OnTutorial();
		List<Objective> objectives = new List<Objective>();
		UIObjectivesController component = objectivesView.GetComponent<UIObjectivesController>();
		component.SetObjectives(objectives, loading: true);
		objectivesView.gameObject.SetActive(value: false);
		((Component)(object)deploymentTitle).gameObject.SetActive(value: false);
		((Component)(object)deploymentDesc).gameObject.SetActive(value: false);
		ambush.SetActive(value: false);
		((Component)(object)ambusher).gameObject.SetActive(value: false);
		((Component)(object)ambushed).gameObject.SetActive(value: false);
		skirmishView.SetActive(value: false);
		descriptionTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(missionData.Name + "_title"));
		descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(missionData.Name + "_desc"));
		LoadBackground("bg_" + missionData.Name + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionData.LoadingImageCount));
		LoadDialog(missionData.Name + "_desc");
	}

	public void OnMission()
	{
		missionHeaderView.OnMission();
		((Component)(object)descriptionTitle).gameObject.SetActive(value: false);
		((Component)(object)descriptionDesc).gameObject.SetActive(value: false);
		ambush.SetActive(value: false);
		((Component)(object)ambusher).gameObject.SetActive(value: false);
		((Component)(object)ambushed).gameObject.SetActive(value: false);
		skirmishView.SetActive(value: false);
		int index = 0;
		if (!PandoraSingleton<Hermes>.Instance.IsHost())
		{
			index = 1;
		}
		DeploymentScenarioSlotData deploymentScenarioSlotData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>(mission.missionSave.deployScenarioSlotIds[index]);
		deploymentTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(deploymentScenarioSlotData.Title));
		deploymentDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(deploymentScenarioSlotData.Setup));
		if (!mission.missionSave.isSkirmish)
		{
			if (deploymentScenarioSlotData.Setup.Contains("ambush_1") || deploymentScenarioSlotData.Setup.Contains("scouting_wagon_1"))
			{
				ambush.SetActive(value: true);
				((Component)(object)ambusher).gameObject.SetActive(value: true);
				ambush.GetComponentInChildren<Text>().set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_ambusher_desc"));
				LoadDialog("mission_ambusher_desc");
			}
			if (deploymentScenarioSlotData.Setup.Contains("ambush_2") || deploymentScenarioSlotData.Setup.Contains("scouting_wagon_2"))
			{
				ambush.SetActive(value: true);
				((Component)(object)ambushed).gameObject.SetActive(value: true);
				ambush.GetComponentInChildren<Text>().set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_ambushed_desc"));
				LoadDialog("mission_ambushed_desc");
			}
		}
		if (audioSrc.clip == null)
		{
			LoadDialog(deploymentScenarioSlotData.Setup);
		}
		objectivesView.SetActive(value: true);
		UIObjectivesController component = objectivesView.GetComponent<UIObjectivesController>();
		List<Objective> objectives = new List<Objective>();
		MissionStartData instance = PandoraSingleton<MissionStartData>.Instance;
		int warbandIndex = PandoraSingleton<MissionStartData>.Instance.GetWarbandIndex(PandoraSingleton<Hermes>.Instance.PlayerIndex);
		Objective.CreateLoadingObjective(ref objectives, (PrimaryObjectiveTypeId)instance.CurrentMission.missionSave.objectiveTypeIds[warbandIndex], instance.FightingWarbands[instance.CurrentMission.missionSave.objectiveTargets[warbandIndex]], instance.CurrentMission.missionSave.objectiveSeeds[warbandIndex]);
		component.SetObjectives(objectives, loading: true);
		Image[] componentsInChildren = objectivesView.GetComponentsInChildren<Image>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (((Component)(object)componentsInChildren[i]).gameObject.name != "tab_cat_line")
			{
				componentsInChildren[i].set_sprite(bulletPoint);
				Vector3 localScale = ((Component)(object)componentsInChildren[i]).transform.localScale;
				localScale.x = 0.8f;
				localScale.y = 0.8f;
				((Component)(object)componentsInChildren[i]).transform.localScale = localScale;
			}
		}
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(mission.missionSave.deployScenarioMapLayoutId);
		MissionMapData missionMapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deploymentScenarioMapLayoutData.MissionMapId);
		if (missionMapData.Name.Contains("01_proc"))
		{
			LoadBackground("bg_grnd_dis_01_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
		else if (missionMapData.Name.Contains("02_proc"))
		{
			LoadBackground("bg_grnd_dis_02_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
		else
		{
			LoadBackground("bg_" + missionMapData.Name + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
	}

	public void OnSkirmish()
	{
		missionHeaderView.OnSkirmish();
		((Component)(object)descriptionTitle).gameObject.SetActive(value: false);
		((Component)(object)descriptionDesc).gameObject.SetActive(value: false);
		ambush.SetActive(value: false);
		((Component)(object)ambusher).gameObject.SetActive(value: false);
		((Component)(object)ambushed).gameObject.SetActive(value: false);
		((Component)(object)deploymentTitle).gameObject.SetActive(value: false);
		((Component)(object)deploymentDesc).gameObject.SetActive(value: false);
		UIObjectivesController component = objectivesView.GetComponent<UIObjectivesController>();
		component.SetObjectives(null, loading: true);
		objectivesView.SetActive(value: false);
		skirmishView.SetActive(value: true);
		campaignDescription.gameObject.SetActive(value: false);
		int num = 0;
		int num2 = 1;
		if (!PandoraSingleton<Hermes>.Instance.IsHost())
		{
			num2 = 0;
			num = 1;
		}
		if (num < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count)
		{
			MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[num];
			warbandName.set_text(missionWarbandSave.Name);
			warbandRank.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_rank_colon_value", missionWarbandSave.Rank.ToString()));
			warbandRating.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_rating_value", missionWarbandSave.Rating.ToString()));
			warbandFlag.set_sprite(Warband.GetIcon(missionWarbandSave.WarbandId));
		}
		if (num2 < PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count)
		{
			MissionWarbandSave missionWarbandSave2 = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[num2];
			opponentWarbandName.set_text(missionWarbandSave2.Name);
			opponentWarbandRank.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_rank_colon_value", missionWarbandSave2.Rank.ToString()));
			opponentWarbandRating.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_rating_value", missionWarbandSave2.Rating.ToString()));
			opponentWarbandFlag.set_sprite(Warband.GetIcon(missionWarbandSave2.WarbandId));
		}
		else
		{
			opponentWarbandName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_ai"));
			opponentWarbandRank.set_text("0");
			opponentWarbandRating.set_text("0");
			opponentWarbandFlag.set_sprite((Sprite)null);
		}
		gameplayType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + ((MissionMapGameplayId)mission.missionSave.mapGameplayId).ToLowerString()));
		gameplayTypeDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_desc_" + ((MissionMapGameplayId)mission.missionSave.mapGameplayId).ToLowerString()));
		DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(mission.missionSave.deployScenarioMapLayoutId);
		deploymentType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + deploymentScenarioMapLayoutData.DeploymentScenarioId.ToLowerString()));
		deploymentTypeDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_desc_" + deploymentScenarioMapLayoutData.DeploymentScenarioId.ToLowerString()));
		optionalObjectivePlayerName.set_text(PandoraSingleton<Hephaestus>.Instance.GetUserName());
		optionalObjective.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + ((PrimaryObjectiveTypeId)mission.missionSave.objectiveTypeIds[num]).ToLowerString()));
		optionalObjectiveOpponentName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_opponent"));
		optionalObjectiveOpponent.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + ((PrimaryObjectiveTypeId)mission.missionSave.objectiveTypeIds[num2]).ToLowerString()));
		MissionMapData missionMapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deploymentScenarioMapLayoutData.MissionMapId);
		if (missionMapData.Name.Contains("01_proc"))
		{
			LoadBackground("bg_grnd_dis_01_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
		else if (missionMapData.Name.Contains("02_proc"))
		{
			LoadBackground("bg_grnd_dis_02_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
		else
		{
			LoadBackground("bg_" + missionMapData.Name + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionMapData.LoadingImageCount));
		}
	}
}
