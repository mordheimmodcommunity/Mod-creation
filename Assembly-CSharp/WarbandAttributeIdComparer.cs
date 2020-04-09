using System.Collections.Generic;

public class WarbandAttributeIdComparer : IEqualityComparer<WarbandAttributeId>
{
	public static readonly WarbandAttributeIdComparer Instance = new WarbandAttributeIdComparer();

	public bool Equals(WarbandAttributeId x, WarbandAttributeId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandAttributeId obj)
	{
		return (int)obj;
	}
}
