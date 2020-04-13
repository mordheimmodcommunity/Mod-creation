using System.Collections.Generic;

public class PropRestrictionIdComparer : IEqualityComparer<PropRestrictionId>
{
    public static readonly PropRestrictionIdComparer Instance = new PropRestrictionIdComparer();

    public bool Equals(PropRestrictionId x, PropRestrictionId y)
    {
        return x == y;
    }

    public int GetHashCode(PropRestrictionId obj)
    {
        return (int)obj;
    }
}
