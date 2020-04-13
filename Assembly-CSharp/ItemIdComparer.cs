using System.Collections.Generic;

public class ItemIdComparer : IEqualityComparer<ItemId>
{
    public static readonly ItemIdComparer Instance = new ItemIdComparer();

    public bool Equals(ItemId x, ItemId y)
    {
        return x == y;
    }

    public int GetHashCode(ItemId obj)
    {
        return (int)obj;
    }
}
