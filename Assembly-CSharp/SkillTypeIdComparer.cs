using System.Collections.Generic;

public class SkillTypeIdComparer : IEqualityComparer<SkillTypeId>
{
    public static readonly SkillTypeIdComparer Instance = new SkillTypeIdComparer();

    public bool Equals(SkillTypeId x, SkillTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(SkillTypeId obj)
    {
        return (int)obj;
    }
}
