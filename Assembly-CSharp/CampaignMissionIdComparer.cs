using System.Collections.Generic;

public class CampaignMissionIdComparer : IEqualityComparer<CampaignMissionId>
{
    public static readonly CampaignMissionIdComparer Instance = new CampaignMissionIdComparer();

    public bool Equals(CampaignMissionId x, CampaignMissionId y)
    {
        return x == y;
    }

    public int GetHashCode(CampaignMissionId obj)
    {
        return (int)obj;
    }
}
