using System.Collections.Generic;

public class SkillAiFilterIdComparer : IEqualityComparer<SkillAiFilterId>
{
    public static readonly SkillAiFilterIdComparer Instance = new SkillAiFilterIdComparer();

    public bool Equals(SkillAiFilterId x, SkillAiFilterId y)
    {
        return x == y;
    }

    public int GetHashCode(SkillAiFilterId obj)
    {
        return (int)obj;
    }
}
