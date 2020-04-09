using System.Collections.Generic;

public class RaceIdComparer : IEqualityComparer<RaceId>
{
	public static readonly RaceIdComparer Instance = new RaceIdComparer();

	public bool Equals(RaceId x, RaceId y)
	{
		return x == y;
	}

	public int GetHashCode(RaceId obj)
	{
		return (int)obj;
	}
}
