using System.Collections.Generic;

public class AttributeIdComparer : IEqualityComparer<AttributeId>
{
    public static readonly AttributeIdComparer Instance = new AttributeIdComparer();

    public bool Equals(AttributeId x, AttributeId y)
    {
        return x == y;
    }

    public int GetHashCode(AttributeId obj)
    {
        return (int)obj;
    }
}
