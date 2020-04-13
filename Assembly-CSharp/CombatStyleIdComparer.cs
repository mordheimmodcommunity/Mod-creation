using System.Collections.Generic;

public class CombatStyleIdComparer : IEqualityComparer<CombatStyleId>
{
    public static readonly CombatStyleIdComparer Instance = new CombatStyleIdComparer();

    public bool Equals(CombatStyleId x, CombatStyleId y)
    {
        return x == y;
    }

    public int GetHashCode(CombatStyleId obj)
    {
        return (int)obj;
    }
}
