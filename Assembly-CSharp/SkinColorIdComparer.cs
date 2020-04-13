using System.Collections.Generic;

public class SkinColorIdComparer : IEqualityComparer<SkinColorId>
{
    public static readonly SkinColorIdComparer Instance = new SkinColorIdComparer();

    public bool Equals(SkinColorId x, SkinColorId y)
    {
        return x == y;
    }

    public int GetHashCode(SkinColorId obj)
    {
        return (int)obj;
    }
}
