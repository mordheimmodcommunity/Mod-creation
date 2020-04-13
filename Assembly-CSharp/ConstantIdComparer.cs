using System.Collections.Generic;

public class ConstantIdComparer : IEqualityComparer<ConstantId>
{
    public static readonly ConstantIdComparer Instance = new ConstantIdComparer();

    public bool Equals(ConstantId x, ConstantId y)
    {
        return x == y;
    }

    public int GetHashCode(ConstantId obj)
    {
        return (int)obj;
    }
}
