using System.Collections.Generic;

public class ZoneAoeIdComparer : IEqualityComparer<ZoneAoeId>
{
	public static readonly ZoneAoeIdComparer Instance = new ZoneAoeIdComparer();

	public bool Equals(ZoneAoeId x, ZoneAoeId y)
	{
		return x == y;
	}

	public int GetHashCode(ZoneAoeId obj)
	{
		return (int)obj;
	}
}
