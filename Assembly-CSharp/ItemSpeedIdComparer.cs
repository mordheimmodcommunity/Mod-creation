using System.Collections.Generic;

public class ItemSpeedIdComparer : IEqualityComparer<ItemSpeedId>
{
    public static readonly ItemSpeedIdComparer Instance = new ItemSpeedIdComparer();

    public bool Equals(ItemSpeedId x, ItemSpeedId y)
    {
        return x == y;
    }

    public int GetHashCode(ItemSpeedId obj)
    {
        return (int)obj;
    }
}
