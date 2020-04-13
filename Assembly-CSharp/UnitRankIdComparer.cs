using System.Collections.Generic;

public class UnitRankIdComparer : IEqualityComparer<UnitRankId>
{
    public static readonly UnitRankIdComparer Instance = new UnitRankIdComparer();

    public bool Equals(UnitRankId x, UnitRankId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitRankId obj)
    {
        return (int)obj;
    }
}
