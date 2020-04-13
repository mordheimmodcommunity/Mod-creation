using System.Collections.Generic;

public class AchievementTypeIdComparer : IEqualityComparer<AchievementTypeId>
{
    public static readonly AchievementTypeIdComparer Instance = new AchievementTypeIdComparer();

    public bool Equals(AchievementTypeId x, AchievementTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(AchievementTypeId obj)
    {
        return (int)obj;
    }
}
