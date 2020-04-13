using System.Collections.Generic;

public class AchievementIdComparer : IEqualityComparer<AchievementId>
{
    public static readonly AchievementIdComparer Instance = new AchievementIdComparer();

    public bool Equals(AchievementId x, AchievementId y)
    {
        return x == y;
    }

    public int GetHashCode(AchievementId obj)
    {
        return (int)obj;
    }
}
