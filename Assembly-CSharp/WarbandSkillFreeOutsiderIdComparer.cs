using System.Collections.Generic;

public class WarbandSkillFreeOutsiderIdComparer : IEqualityComparer<WarbandSkillFreeOutsiderId>
{
    public static readonly WarbandSkillFreeOutsiderIdComparer Instance = new WarbandSkillFreeOutsiderIdComparer();

    public bool Equals(WarbandSkillFreeOutsiderId x, WarbandSkillFreeOutsiderId y)
    {
        return x == y;
    }

    public int GetHashCode(WarbandSkillFreeOutsiderId obj)
    {
        return (int)obj;
    }
}
