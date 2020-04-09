using System.Collections.Generic;

public class EnchantmentDmgTriggerIdComparer : IEqualityComparer<EnchantmentDmgTriggerId>
{
	public static readonly EnchantmentDmgTriggerIdComparer Instance = new EnchantmentDmgTriggerIdComparer();

	public bool Equals(EnchantmentDmgTriggerId x, EnchantmentDmgTriggerId y)
	{
		return x == y;
	}

	public int GetHashCode(EnchantmentDmgTriggerId obj)
	{
		return (int)obj;
	}
}
