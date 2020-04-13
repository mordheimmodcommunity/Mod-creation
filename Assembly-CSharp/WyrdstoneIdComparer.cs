using System.Collections.Generic;

public class WyrdstoneIdComparer : IEqualityComparer<WyrdstoneId>
{
    public static readonly WyrdstoneIdComparer Instance = new WyrdstoneIdComparer();

    public bool Equals(WyrdstoneId x, WyrdstoneId y)
    {
        return x == y;
    }

    public int GetHashCode(WyrdstoneId obj)
    {
        return (int)obj;
    }
}
