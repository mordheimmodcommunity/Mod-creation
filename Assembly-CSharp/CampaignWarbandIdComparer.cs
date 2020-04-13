using System.Collections.Generic;

public class CampaignWarbandIdComparer : IEqualityComparer<CampaignWarbandId>
{
    public static readonly CampaignWarbandIdComparer Instance = new CampaignWarbandIdComparer();

    public bool Equals(CampaignWarbandId x, CampaignWarbandId y)
    {
        return x == y;
    }

    public int GetHashCode(CampaignWarbandId obj)
    {
        return (int)obj;
    }
}
