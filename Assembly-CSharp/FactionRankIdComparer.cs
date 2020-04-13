using System.Collections.Generic;

public class FactionRankIdComparer : IEqualityComparer<FactionRankId>
{
    public static readonly FactionRankIdComparer Instance = new FactionRankIdComparer();

    public bool Equals(FactionRankId x, FactionRankId y)
    {
        return x == y;
    }

    public int GetHashCode(FactionRankId obj)
    {
        return (int)obj;
    }
}
