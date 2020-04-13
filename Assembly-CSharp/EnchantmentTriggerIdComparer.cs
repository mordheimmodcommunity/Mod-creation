using System.Collections.Generic;

public class EnchantmentTriggerIdComparer : IEqualityComparer<EnchantmentTriggerId>
{
    public static readonly EnchantmentTriggerIdComparer Instance = new EnchantmentTriggerIdComparer();

    public bool Equals(EnchantmentTriggerId x, EnchantmentTriggerId y)
    {
        return x == y;
    }

    public int GetHashCode(EnchantmentTriggerId obj)
    {
        return (int)obj;
    }
}
