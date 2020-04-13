using System.Collections.Generic;

public class UnitIdComparer : IEqualityComparer<UnitId>
{
    public static readonly UnitIdComparer Instance = new UnitIdComparer();

    public bool Equals(UnitId x, UnitId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitId obj)
    {
        return (int)obj;
    }
}
