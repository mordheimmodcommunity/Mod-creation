using System.Collections.Generic;

public class TrapTypeIdComparer : IEqualityComparer<TrapTypeId>
{
    public static readonly TrapTypeIdComparer Instance = new TrapTypeIdComparer();

    public bool Equals(TrapTypeId x, TrapTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(TrapTypeId obj)
    {
        return (int)obj;
    }
}
