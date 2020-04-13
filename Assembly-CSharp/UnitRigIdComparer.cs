using System.Collections.Generic;

public class UnitRigIdComparer : IEqualityComparer<UnitRigId>
{
    public static readonly UnitRigIdComparer Instance = new UnitRigIdComparer();

    public bool Equals(UnitRigId x, UnitRigId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitRigId obj)
    {
        return (int)obj;
    }
}
