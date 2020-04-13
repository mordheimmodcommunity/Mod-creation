using System.Collections.Generic;

public class CampaignMissionSpawnIdComparer : IEqualityComparer<CampaignMissionSpawnId>
{
    public static readonly CampaignMissionSpawnIdComparer Instance = new CampaignMissionSpawnIdComparer();

    public bool Equals(CampaignMissionSpawnId x, CampaignMissionSpawnId y)
    {
        return x == y;
    }

    public int GetHashCode(CampaignMissionSpawnId obj)
    {
        return (int)obj;
    }
}
