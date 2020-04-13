using System.Collections.Generic;

public class WyrdstonePlacementIdComparer : IEqualityComparer<WyrdstonePlacementId>
{
    public static readonly WyrdstonePlacementIdComparer Instance = new WyrdstonePlacementIdComparer();

    public bool Equals(WyrdstonePlacementId x, WyrdstonePlacementId y)
    {
        return x == y;
    }

    public int GetHashCode(WyrdstonePlacementId obj)
    {
        return (int)obj;
    }
}
