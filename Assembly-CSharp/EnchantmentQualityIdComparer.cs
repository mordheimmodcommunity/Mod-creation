using System.Collections.Generic;

public class EnchantmentQualityIdComparer : IEqualityComparer<EnchantmentQualityId>
{
    public static readonly EnchantmentQualityIdComparer Instance = new EnchantmentQualityIdComparer();

    public bool Equals(EnchantmentQualityId x, EnchantmentQualityId y)
    {
        return x == y;
    }

    public int GetHashCode(EnchantmentQualityId obj)
    {
        return (int)obj;
    }
}
