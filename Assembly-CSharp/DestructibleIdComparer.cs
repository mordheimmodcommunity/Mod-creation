using System.Collections.Generic;

public class DestructibleIdComparer : IEqualityComparer<DestructibleId>
{
    public static readonly DestructibleIdComparer Instance = new DestructibleIdComparer();

    public bool Equals(DestructibleId x, DestructibleId y)
    {
        return x == y;
    }

    public int GetHashCode(DestructibleId obj)
    {
        return (int)obj;
    }
}
