using System.Collections.Generic;

public class ProcMissionRatingIdComparer : IEqualityComparer<ProcMissionRatingId>
{
    public static readonly ProcMissionRatingIdComparer Instance = new ProcMissionRatingIdComparer();

    public bool Equals(ProcMissionRatingId x, ProcMissionRatingId y)
    {
        return x == y;
    }

    public int GetHashCode(ProcMissionRatingId obj)
    {
        return (int)obj;
    }
}
