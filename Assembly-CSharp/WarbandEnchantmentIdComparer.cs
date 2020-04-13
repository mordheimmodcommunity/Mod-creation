using System.Collections.Generic;

public class WarbandEnchantmentIdComparer : IEqualityComparer<WarbandEnchantmentId>
{
    public static readonly WarbandEnchantmentIdComparer Instance = new WarbandEnchantmentIdComparer();

    public bool Equals(WarbandEnchantmentId x, WarbandEnchantmentId y)
    {
        return x == y;
    }

    public int GetHashCode(WarbandEnchantmentId obj)
    {
        return (int)obj;
    }
}
