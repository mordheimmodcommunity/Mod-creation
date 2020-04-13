using System.Collections.Generic;

public class UnitTypeIdComparer : IEqualityComparer<UnitTypeId>
{
    public static readonly UnitTypeIdComparer Instance = new UnitTypeIdComparer();

    public bool Equals(UnitTypeId x, UnitTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitTypeId obj)
    {
        return (int)obj;
    }
}
