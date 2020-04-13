using System.Collections.Generic;

public class SpellTypeIdComparer : IEqualityComparer<SpellTypeId>
{
    public static readonly SpellTypeIdComparer Instance = new SpellTypeIdComparer();

    public bool Equals(SpellTypeId x, SpellTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(SpellTypeId obj)
    {
        return (int)obj;
    }
}
