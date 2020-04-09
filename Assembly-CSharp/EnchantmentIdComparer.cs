using System.Collections.Generic;

public class EnchantmentIdComparer : IEqualityComparer<EnchantmentId>
{
	public static readonly EnchantmentIdComparer Instance = new EnchantmentIdComparer();

	public bool Equals(EnchantmentId x, EnchantmentId y)
	{
		return x == y;
	}

	public int GetHashCode(EnchantmentId obj)
	{
		return (int)obj;
	}
}
