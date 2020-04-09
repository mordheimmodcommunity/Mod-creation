using System.Collections.Generic;

public class ItemCategoryIdComparer : IEqualityComparer<ItemCategoryId>
{
	public static readonly ItemCategoryIdComparer Instance = new ItemCategoryIdComparer();

	public bool Equals(ItemCategoryId x, ItemCategoryId y)
	{
		return x == y;
	}

	public int GetHashCode(ItemCategoryId obj)
	{
		return (int)obj;
	}
}
