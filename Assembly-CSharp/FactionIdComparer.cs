using System.Collections.Generic;

public class FactionIdComparer : IEqualityComparer<FactionId>
{
    public static readonly FactionIdComparer Instance = new FactionIdComparer();

    public bool Equals(FactionId x, FactionId y)
    {
        return x == y;
    }

    public int GetHashCode(FactionId obj)
    {
        return (int)obj;
    }
}
