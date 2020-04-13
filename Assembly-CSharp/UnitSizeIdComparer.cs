using System.Collections.Generic;

public class UnitSizeIdComparer : IEqualityComparer<UnitSizeId>
{
    public static readonly UnitSizeIdComparer Instance = new UnitSizeIdComparer();

    public bool Equals(UnitSizeId x, UnitSizeId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitSizeId obj)
    {
        return (int)obj;
    }
}
