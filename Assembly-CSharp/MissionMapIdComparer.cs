using System.Collections.Generic;

public class MissionMapIdComparer : IEqualityComparer<MissionMapId>
{
    public static readonly MissionMapIdComparer Instance = new MissionMapIdComparer();

    public bool Equals(MissionMapId x, MissionMapId y)
    {
        return x == y;
    }

    public int GetHashCode(MissionMapId obj)
    {
        return (int)obj;
    }
}
