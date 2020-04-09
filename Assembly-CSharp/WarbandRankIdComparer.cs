using System.Collections.Generic;

public class WarbandRankIdComparer : IEqualityComparer<WarbandRankId>
{
	public static readonly WarbandRankIdComparer Instance = new WarbandRankIdComparer();

	public bool Equals(WarbandRankId x, WarbandRankId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandRankId obj)
	{
		return (int)obj;
	}
}
