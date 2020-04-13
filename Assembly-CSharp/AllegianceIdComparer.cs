using System.Collections.Generic;

public class AllegianceIdComparer : IEqualityComparer<AllegianceId>
{
    public static readonly AllegianceIdComparer Instance = new AllegianceIdComparer();

    public bool Equals(AllegianceId x, AllegianceId y)
    {
        return x == y;
    }

    public int GetHashCode(AllegianceId obj)
    {
        return (int)obj;
    }
}
