using System.Collections.Generic;

public class ShopEventIdComparer : IEqualityComparer<ShopEventId>
{
	public static readonly ShopEventIdComparer Instance = new ShopEventIdComparer();

	public bool Equals(ShopEventId x, ShopEventId y)
	{
		return x == y;
	}

	public int GetHashCode(ShopEventId obj)
	{
		return (int)obj;
	}
}
