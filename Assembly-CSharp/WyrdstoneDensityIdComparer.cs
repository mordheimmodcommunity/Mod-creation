using System.Collections.Generic;

public class WyrdstoneDensityIdComparer : IEqualityComparer<WyrdstoneDensityId>
{
    public static readonly WyrdstoneDensityIdComparer Instance = new WyrdstoneDensityIdComparer();

    public bool Equals(WyrdstoneDensityId x, WyrdstoneDensityId y)
    {
        return x == y;
    }

    public int GetHashCode(WyrdstoneDensityId obj)
    {
        return (int)obj;
    }
}
