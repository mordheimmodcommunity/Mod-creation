using System.Collections.Generic;

public class UnitWoundIdComparer : IEqualityComparer<UnitWoundId>
{
    public static readonly UnitWoundIdComparer Instance = new UnitWoundIdComparer();

    public bool Equals(UnitWoundId x, UnitWoundId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitWoundId obj)
    {
        return (int)obj;
    }
}
