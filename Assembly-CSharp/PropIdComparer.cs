using System.Collections.Generic;

public class PropIdComparer : IEqualityComparer<PropId>
{
    public static readonly PropIdComparer Instance = new PropIdComparer();

    public bool Equals(PropId x, PropId y)
    {
        return x == y;
    }

    public int GetHashCode(PropId obj)
    {
        return (int)obj;
    }
}
