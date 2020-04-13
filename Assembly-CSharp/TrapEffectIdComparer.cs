using System.Collections.Generic;

public class TrapEffectIdComparer : IEqualityComparer<TrapEffectId>
{
    public static readonly TrapEffectIdComparer Instance = new TrapEffectIdComparer();

    public bool Equals(TrapEffectId x, TrapEffectId y)
    {
        return x == y;
    }

    public int GetHashCode(TrapEffectId obj)
    {
        return (int)obj;
    }
}
