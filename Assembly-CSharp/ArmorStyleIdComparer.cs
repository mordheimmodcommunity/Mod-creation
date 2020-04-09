using System.Collections.Generic;

public class ArmorStyleIdComparer : IEqualityComparer<ArmorStyleId>
{
	public static readonly ArmorStyleIdComparer Instance = new ArmorStyleIdComparer();

	public bool Equals(ArmorStyleId x, ArmorStyleId y)
	{
		return x == y;
	}

	public int GetHashCode(ArmorStyleId obj)
	{
		return (int)obj;
	}
}
