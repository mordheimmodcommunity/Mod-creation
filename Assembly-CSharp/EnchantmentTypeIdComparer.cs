using System.Collections.Generic;

public class EnchantmentTypeIdComparer : IEqualityComparer<EnchantmentTypeId>
{
    public static readonly EnchantmentTypeIdComparer Instance = new EnchantmentTypeIdComparer();

    public bool Equals(EnchantmentTypeId x, EnchantmentTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(EnchantmentTypeId obj)
    {
        return (int)obj;
    }
}
