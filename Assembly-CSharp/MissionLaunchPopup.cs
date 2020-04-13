using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionLaunchPopup : UIPopupModule
{
    public Text missionName;

    public Text wyrdstones;

    public Text treasures;

    public Text description;

    public Text rating;

    public Text actText;

    public Text difficulty;

    public Image diffIcon;

    public Image icon;

    public GameObject resources;

    public void Setup(Mission miss)
    {
        if (miss.missionSave.isCampaign)
        {
            List<CampaignMissionData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(new string[2]
            {
                "fk_warband_id",
                "idx"
            }, new string[2]
            {
                ((int)PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id).ToString(),
                PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx.ToString()
            });
            missionName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_title_" + list[0].Name));
            description.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_announcement_short_" + list[0].Name));
            actText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("act_" + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx));
            resources.SetActive(value: false);
        }
        else
        {
            missionName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + miss.GetDeploymentScenarioId().ToLowerString()));
            actText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + ((PrimaryObjectiveTypeId)miss.missionSave.objectiveTypeIds[0]).ToString()));
            resources.SetActive(value: true);
            description.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_desc_" + miss.GetDeploymentScenarioId().ToLowerString()));
        }
        difficulty.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_difficulty_" + ((PrimaryObjectiveId)miss.missionSave.ratingId).ToString()));
        SetupShort(miss);
    }

    public void SetupShort(Mission miss)
    {
        diffIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_mission_difficulty_" + ((PrimaryObjectiveId)miss.missionSave.ratingId).ToString(), cached: true));
        wyrdstones.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("yield_" + ((WyrdstoneDensityId)miss.missionSave.wyrdDensityId).ToString()));
        treasures.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("yield_" + ((SearchDensityId)miss.missionSave.searchDensityId).ToString()));
        rating.set_text(miss.missionSave.rating.ToString());
    }
}
