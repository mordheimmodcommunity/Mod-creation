using System.Collections.Generic;

public class WarbandIdComparer : IEqualityComparer<WarbandId>
{
	public static readonly WarbandIdComparer Instance = new WarbandIdComparer();

	public bool Equals(WarbandId x, WarbandId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandId obj)
	{
		return (int)obj;
	}
}
