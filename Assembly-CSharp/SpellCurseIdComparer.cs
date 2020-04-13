using System.Collections.Generic;

public class SpellCurseIdComparer : IEqualityComparer<SpellCurseId>
{
    public static readonly SpellCurseIdComparer Instance = new SpellCurseIdComparer();

    public bool Equals(SpellCurseId x, SpellCurseId y)
    {
        return x == y;
    }

    public int GetHashCode(SpellCurseId obj)
    {
        return (int)obj;
    }
}
