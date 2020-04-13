using System.Collections.Generic;

public class SkillQualityIdComparer : IEqualityComparer<SkillQualityId>
{
    public static readonly SkillQualityIdComparer Instance = new SkillQualityIdComparer();

    public bool Equals(SkillQualityId x, SkillQualityId y)
    {
        return x == y;
    }

    public int GetHashCode(SkillQualityId obj)
    {
        return (int)obj;
    }
}
