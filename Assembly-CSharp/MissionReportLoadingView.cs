using UnityEngine;
using UnityEngine.UI;

public class MissionReportLoadingView : LoadingView
{
    public MissionHeaderView missionHeaderView;

    public Text descriptionTitle;

    public Text descriptionDesc;

    public Image flag;

    private Mission mission;

    private CampaignMissionData missionData;

    private MissionEndDataSave missionEndData;

    public override void Show()
    {
        base.Show();
        missionEndData = PandoraSingleton<GameManager>.Instance.currentSave.endMission;
        mission = new Mission(missionEndData.missionSave);
        missionData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(mission.missionSave.campaignId);
        missionHeaderView.Init(mission, missionData);
        descriptionTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_victory_" + missionEndData.VictoryType.ToLowerString()));
        if (mission.missionSave.isCampaign)
        {
            OnCampaign();
        }
        else
        {
            OnMission();
        }
    }

    public void OnCampaign()
    {
        missionHeaderView.OnCampaign();
        if (missionEndData.primaryObjectiveCompleted)
        {
            descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_camp_epilogue_" + missionData.Name));
            LoadDialog("mission_camp_epilogue_" + missionData.Name);
        }
        else
        {
            descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_defeated_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString()));
            LoadDialog("mission_defeated_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString());
        }
        SetFlag();
        LoadBackground("bg_end_" + missionData.Name + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, missionData.LoadingImageCount));
    }

    public void OnMission()
    {
        missionHeaderView.OnMission();
        DeploymentScenarioMapLayoutData deploymentScenarioMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>(mission.missionSave.deployScenarioMapLayoutId);
        MissionMapData missionMapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)deploymentScenarioMapLayoutData.MissionMapId);
        if (missionEndData.VictoryType != 0)
        {
            string text = "mission_victory_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString() + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 4);
            descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(text));
            LoadDialog(text);
            int exclusiveBound = 4;
            if (missionMapData.Name.Contains("01_proc"))
            {
                LoadBackground("bg_win_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString() + "_dis_01_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, exclusiveBound));
            }
            else if (missionMapData.Name.Contains("02_proc"))
            {
                LoadBackground("bg_win_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString() + "_dis_02_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, exclusiveBound));
            }
            else
            {
                LoadBackground("bg_win_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString() + "_dis_01_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, exclusiveBound));
            }
        }
        else
        {
            string text2 = "mission_defeated_" + ((WarbandId)PandoraSingleton<GameManager>.Instance.currentSave.id).ToLowerString() + "_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 4);
            descriptionDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(text2));
            LoadDialog(text2);
            int exclusiveBound2 = 8;
            LoadBackground("bg_lose_" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, exclusiveBound2));
        }
        SetFlag();
    }

    private void SetFlag()
    {
        ((Component)(object)flag).gameObject.SetActive(value: false);
    }
}
