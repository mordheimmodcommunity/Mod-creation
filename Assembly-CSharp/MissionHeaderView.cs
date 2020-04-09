using UnityEngine;
using UnityEngine.UI;

public class MissionHeaderView : MonoBehaviour
{
	public ButtonMapView difficultyGroup;

	public Text txtAct;

	public Text title;

	public ButtonMapView missionGroup;

	public ButtonMapView wyrdstoneGroup;

	public ButtonMapView searchGroup;

	private Mission _mission;

	private CampaignMissionData _missionData;

	public void Init(Mission mission, CampaignMissionData missionData)
	{
		_mission = mission;
		_missionData = missionData;
	}

	public void OnCampaign()
	{
		wyrdstoneGroup.gameObject.SetActive(value: false);
		searchGroup.gameObject.SetActive(value: false);
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_title_" + _missionData.Name));
		txtAct.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_act_" + _missionData.Name.Substring(_missionData.Name.LastIndexOf('_') - 1)));
		SetDifficulty();
		SetMission();
	}

	public void OnTutorial()
	{
		difficultyGroup.gameObject.SetActive(value: false);
		((Behaviour)(object)txtAct).enabled = false;
		missionGroup.gameObject.SetActive(value: false);
		wyrdstoneGroup.gameObject.SetActive(value: false);
		searchGroup.gameObject.SetActive(value: false);
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(_missionData.Name + "_title"));
	}

	public void OnMission()
	{
		((Behaviour)(object)txtAct).enabled = false;
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("district_" + _mission.GetDistrictId().ToLowerString()));
		SetDifficulty();
		SetMission();
		searchGroup.text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("yield_" + ((SearchDensityId)_mission.missionSave.searchDensityId).ToLowerString()));
		wyrdstoneGroup.text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("yield_" + ((WyrdstoneDensityId)_mission.missionSave.wyrdDensityId).ToLowerString()));
	}

	public void OnSkirmish()
	{
		((Behaviour)(object)txtAct).enabled = false;
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("district_" + _mission.GetDistrictId().ToLowerString()));
		searchGroup.gameObject.SetActive(value: false);
		missionGroup.gameObject.SetActive(value: false);
		wyrdstoneGroup.gameObject.SetActive(value: false);
	}

	private void SetMission()
	{
		missionGroup.gameObject.SetActive(value: true);
		if (_mission.missionSave.ratingId != 0)
		{
			missionGroup.text.set_text(((ProcMissionRatingId)_mission.missionSave.rating).ToString());
		}
		else
		{
			missionGroup.text.set_text(string.Empty);
		}
	}

	private void SetDifficulty()
	{
		difficultyGroup.gameObject.SetActive(value: false);
		if (_mission.missionSave.ratingId != 0)
		{
			difficultyGroup.buttonImage.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_mission_difficulty_" + ((ProcMissionRatingId)_mission.missionSave.ratingId).ToLowerString(), cached: true));
		}
	}
}
