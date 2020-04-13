using System.Collections.Generic;

public class RuneMarkQualityIdComparer : IEqualityComparer<RuneMarkQualityId>
{
    public static readonly RuneMarkQualityIdComparer Instance = new RuneMarkQualityIdComparer();

    public bool Equals(RuneMarkQualityId x, RuneMarkQualityId y)
    {
        return x == y;
    }

    public int GetHashCode(RuneMarkQualityId obj)
    {
        return (int)obj;
    }
}
