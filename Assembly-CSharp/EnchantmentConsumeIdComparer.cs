using System.Collections.Generic;

public class EnchantmentConsumeIdComparer : IEqualityComparer<EnchantmentConsumeId>
{
	public static readonly EnchantmentConsumeIdComparer Instance = new EnchantmentConsumeIdComparer();

	public bool Equals(EnchantmentConsumeId x, EnchantmentConsumeId y)
	{
		return x == y;
	}

	public int GetHashCode(EnchantmentConsumeId obj)
	{
		return (int)obj;
	}
}
