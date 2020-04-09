using System.Collections.Generic;

public class WyrdstoneTypeIdComparer : IEqualityComparer<WyrdstoneTypeId>
{
	public static readonly WyrdstoneTypeIdComparer Instance = new WyrdstoneTypeIdComparer();

	public bool Equals(WyrdstoneTypeId x, WyrdstoneTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(WyrdstoneTypeId obj)
	{
		return (int)obj;
	}
}
