using System.Collections.Generic;

public class RuneMarkIdComparer : IEqualityComparer<RuneMarkId>
{
    public static readonly RuneMarkIdComparer Instance = new RuneMarkIdComparer();

    public bool Equals(RuneMarkId x, RuneMarkId y)
    {
        return x == y;
    }

    public int GetHashCode(RuneMarkId obj)
    {
        return (int)obj;
    }
}
