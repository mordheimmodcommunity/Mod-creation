using System.Collections.Generic;

public class EnchantmentComparer : IComparer<Enchantment>
{
	public int Compare(Enchantment x, Enchantment y)
	{
		if (x.Duration < y.Duration)
		{
			return -1;
		}
		if (x.Duration > y.Duration)
		{
			return 1;
		}
		return string.CompareOrdinal(x.Data.Name, y.Data.Name);
	}
}
