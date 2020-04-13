using System.Collections.Generic;

public class UnitStateIdComparer : IEqualityComparer<UnitStateId>
{
    public static readonly UnitStateIdComparer Instance = new UnitStateIdComparer();

    public bool Equals(UnitStateId x, UnitStateId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitStateId obj)
    {
        return (int)obj;
    }
}
