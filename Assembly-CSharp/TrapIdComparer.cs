using System.Collections.Generic;

public class TrapIdComparer : IEqualityComparer<TrapId>
{
    public static readonly TrapIdComparer Instance = new TrapIdComparer();

    public bool Equals(TrapId x, TrapId y)
    {
        return x == y;
    }

    public int GetHashCode(TrapId obj)
    {
        return (int)obj;
    }
}
