using System.Collections.Generic;

public class WarbandNameIdComparer : IEqualityComparer<WarbandNameId>
{
    public static readonly WarbandNameIdComparer Instance = new WarbandNameIdComparer();

    public bool Equals(WarbandNameId x, WarbandNameId y)
    {
        return x == y;
    }

    public int GetHashCode(WarbandNameId obj)
    {
        return (int)obj;
    }
}
