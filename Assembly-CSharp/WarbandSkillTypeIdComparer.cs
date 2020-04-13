using System.Collections.Generic;

public class WarbandSkillTypeIdComparer : IEqualityComparer<WarbandSkillTypeId>
{
    public static readonly WarbandSkillTypeIdComparer Instance = new WarbandSkillTypeIdComparer();

    public bool Equals(WarbandSkillTypeId x, WarbandSkillTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(WarbandSkillTypeId obj)
    {
        return (int)obj;
    }
}
