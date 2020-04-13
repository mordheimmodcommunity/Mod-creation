public class AchievementCampaignMission : Achievement
{
    public AchievementCampaignMission(AchievementData data)
        : base(data)
    {
    }

    public override bool CheckFinishMission(CampaignMissionId missionId)
    {
        return base.Data.CampaignMissionId == missionId;
    }
}
