using System.Collections.Generic;

public class SkillIdComparer : IEqualityComparer<SkillId>
{
    public static readonly SkillIdComparer Instance = new SkillIdComparer();

    public bool Equals(SkillId x, SkillId y)
    {
        return x == y;
    }

    public int GetHashCode(SkillId obj)
    {
        return (int)obj;
    }
}
