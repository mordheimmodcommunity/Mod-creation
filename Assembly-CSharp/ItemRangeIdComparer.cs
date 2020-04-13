using System.Collections.Generic;

public class ItemRangeIdComparer : IEqualityComparer<ItemRangeId>
{
    public static readonly ItemRangeIdComparer Instance = new ItemRangeIdComparer();

    public bool Equals(ItemRangeId x, ItemRangeId y)
    {
        return x == y;
    }

    public int GetHashCode(ItemRangeId obj)
    {
        return (int)obj;
    }
}
