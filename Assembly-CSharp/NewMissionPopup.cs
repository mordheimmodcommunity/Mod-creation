using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewMissionPopup : ConfirmationPopupView
{
    public Image icon;

    public Text rating;

    public Text act;

    public Text difficulty;

    public Image diffIcon;

    public void Setup(Action<bool> callback, bool hideButtons = false)
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
        string titleId = "mission_camp_title_" + list[0].Name;
        string textId = "mission_camp_announcement_" + list[0].Name;
        base.Show(titleId, textId, callback, hideButtons);
        Mission mission = PandoraSingleton<HideoutManager>.Instance.missions[0];
        mission.RefreshDifficulty(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRating(), isProc: false);
        icon.set_sprite(Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id));
        rating.set_text(mission.missionSave.rating.ToString());
        difficulty.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_difficulty_" + ((ProcMissionRatingId)mission.missionSave.ratingId).ToLowerString()));
        diffIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_mission_difficulty_" + ((ProcMissionRatingId)mission.missionSave.ratingId).ToLowerString(), cached: true));
        act.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("act_" + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().curCampaignIdx));
    }
}
