using System.Collections.Generic;

public class AchievementCategoryIdComparer : IEqualityComparer<AchievementCategoryId>
{
    public static readonly AchievementCategoryIdComparer Instance = new AchievementCategoryIdComparer();

    public bool Equals(AchievementCategoryId x, AchievementCategoryId y)
    {
        return x == y;
    }

    public int GetHashCode(AchievementCategoryId obj)
    {
        return (int)obj;
    }
}
