using System.Collections.Generic;

public class SkillLineIdComparer : IEqualityComparer<SkillLineId>
{
    public static readonly SkillLineIdComparer Instance = new SkillLineIdComparer();

    public bool Equals(SkillLineId x, SkillLineId y)
    {
        return x == y;
    }

    public int GetHashCode(SkillLineId obj)
    {
        return (int)obj;
    }
}
