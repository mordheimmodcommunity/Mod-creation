using System.Collections.Generic;

public class MovementIdComparer : IEqualityComparer<MovementId>
{
    public static readonly MovementIdComparer Instance = new MovementIdComparer();

    public bool Equals(MovementId x, MovementId y)
    {
        return x == y;
    }

    public int GetHashCode(MovementId obj)
    {
        return (int)obj;
    }
}
