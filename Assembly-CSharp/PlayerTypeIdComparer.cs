using System.Collections.Generic;

public class PlayerTypeIdComparer : IEqualityComparer<PlayerTypeId>
{
    public static readonly PlayerTypeIdComparer Instance = new PlayerTypeIdComparer();

    public bool Equals(PlayerTypeId x, PlayerTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(PlayerTypeId obj)
    {
        return (int)obj;
    }
}
