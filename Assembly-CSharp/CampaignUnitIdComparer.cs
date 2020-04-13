using System.Collections.Generic;

public class CampaignUnitIdComparer : IEqualityComparer<CampaignUnitId>
{
    public static readonly CampaignUnitIdComparer Instance = new CampaignUnitIdComparer();

    public bool Equals(CampaignUnitId x, CampaignUnitId y)
    {
        return x == y;
    }

    public int GetHashCode(CampaignUnitId obj)
    {
        return (int)obj;
    }
}
