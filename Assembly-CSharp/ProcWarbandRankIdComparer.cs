using System.Collections.Generic;

public class ProcWarbandRankIdComparer : IEqualityComparer<ProcWarbandRankId>
{
	public static readonly ProcWarbandRankIdComparer Instance = new ProcWarbandRankIdComparer();

	public bool Equals(ProcWarbandRankId x, ProcWarbandRankId y)
	{
		return x == y;
	}

	public int GetHashCode(ProcWarbandRankId obj)
	{
		return (int)obj;
	}
}
