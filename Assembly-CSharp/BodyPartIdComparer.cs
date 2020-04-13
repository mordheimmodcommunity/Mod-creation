using System.Collections.Generic;

public class BodyPartIdComparer : IEqualityComparer<BodyPartId>
{
    public static readonly BodyPartIdComparer Instance = new BodyPartIdComparer();

    public bool Equals(BodyPartId x, BodyPartId y)
    {
        return x == y;
    }

    public int GetHashCode(BodyPartId obj)
    {
        return (int)obj;
    }
}
