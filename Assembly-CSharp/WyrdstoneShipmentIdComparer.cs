using System.Collections.Generic;

public class WyrdstoneShipmentIdComparer : IEqualityComparer<WyrdstoneShipmentId>
{
	public static readonly WyrdstoneShipmentIdComparer Instance = new WyrdstoneShipmentIdComparer();

	public bool Equals(WyrdstoneShipmentId x, WyrdstoneShipmentId y)
	{
		return x == y;
	}

	public int GetHashCode(WyrdstoneShipmentId obj)
	{
		return (int)obj;
	}
}
