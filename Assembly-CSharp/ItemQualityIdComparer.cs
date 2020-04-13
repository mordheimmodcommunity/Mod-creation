using System.Collections.Generic;

public class ItemQualityIdComparer : IEqualityComparer<ItemQualityId>
{
    public static readonly ItemQualityIdComparer Instance = new ItemQualityIdComparer();

    public bool Equals(ItemQualityId x, ItemQualityId y)
    {
        return x == y;
    }

    public int GetHashCode(ItemQualityId obj)
    {
        return (int)obj;
    }
}
