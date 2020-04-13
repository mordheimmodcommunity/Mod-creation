using System.Collections.Generic;

public class InjuryIdComparer : IEqualityComparer<InjuryId>
{
    public static readonly InjuryIdComparer Instance = new InjuryIdComparer();

    public bool Equals(InjuryId x, InjuryId y)
    {
        return x == y;
    }

    public int GetHashCode(InjuryId obj)
    {
        return (int)obj;
    }
}
