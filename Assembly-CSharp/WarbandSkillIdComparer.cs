using System.Collections.Generic;

public class WarbandSkillIdComparer : IEqualityComparer<WarbandSkillId>
{
	public static readonly WarbandSkillIdComparer Instance = new WarbandSkillIdComparer();

	public bool Equals(WarbandSkillId x, WarbandSkillId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandSkillId obj)
	{
		return (int)obj;
	}
}
