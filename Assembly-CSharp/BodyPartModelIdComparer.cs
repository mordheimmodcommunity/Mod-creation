using System.Collections.Generic;

public class BodyPartModelIdComparer : IEqualityComparer<BodyPartModelId>
{
    public static readonly BodyPartModelIdComparer Instance = new BodyPartModelIdComparer();

    public bool Equals(BodyPartModelId x, BodyPartModelId y)
    {
        return x == y;
    }

    public int GetHashCode(BodyPartModelId obj)
    {
        return (int)obj;
    }
}
