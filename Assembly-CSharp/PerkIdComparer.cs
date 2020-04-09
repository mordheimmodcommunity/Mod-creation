using System.Collections.Generic;

public class PerkIdComparer : IEqualityComparer<PerkId>
{
	public static readonly PerkIdComparer Instance = new PerkIdComparer();

	public bool Equals(PerkId x, PerkId y)
	{
		return x == y;
	}

	public int GetHashCode(PerkId obj)
	{
		return (int)obj;
	}
}
