using System.Collections.Generic;

public class ShrineIdComparer : IEqualityComparer<ShrineId>
{
	public static readonly ShrineIdComparer Instance = new ShrineIdComparer();

	public bool Equals(ShrineId x, ShrineId y)
	{
		return x == y;
	}

	public int GetHashCode(ShrineId obj)
	{
		return (int)obj;
	}
}
