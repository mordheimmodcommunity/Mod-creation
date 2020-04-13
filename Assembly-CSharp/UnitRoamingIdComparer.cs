using System.Collections.Generic;

public class UnitRoamingIdComparer : IEqualityComparer<UnitRoamingId>
{
    public static readonly UnitRoamingIdComparer Instance = new UnitRoamingIdComparer();

    public bool Equals(UnitRoamingId x, UnitRoamingId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitRoamingId obj)
    {
        return (int)obj;
    }
}
