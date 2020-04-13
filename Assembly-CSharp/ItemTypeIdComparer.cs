using System.Collections.Generic;

public class ItemTypeIdComparer : IEqualityComparer<ItemTypeId>
{
    public static readonly ItemTypeIdComparer Instance = new ItemTypeIdComparer();

    public bool Equals(ItemTypeId x, ItemTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(ItemTypeId obj)
    {
        return (int)obj;
    }
}
