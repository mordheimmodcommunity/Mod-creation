using System.Collections.Generic;

public class CampaignMissionMessageIdComparer : IEqualityComparer<CampaignMissionMessageId>
{
    public static readonly CampaignMissionMessageIdComparer Instance = new CampaignMissionMessageIdComparer();

    public bool Equals(CampaignMissionMessageId x, CampaignMissionMessageId y)
    {
        return x == y;
    }

    public int GetHashCode(CampaignMissionMessageId obj)
    {
        return (int)obj;
    }
}
