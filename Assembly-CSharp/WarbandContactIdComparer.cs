using System.Collections.Generic;

public class WarbandContactIdComparer : IEqualityComparer<WarbandContactId>
{
	public static readonly WarbandContactIdComparer Instance = new WarbandContactIdComparer();

	public bool Equals(WarbandContactId x, WarbandContactId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandContactId obj)
	{
		return (int)obj;
	}
}
