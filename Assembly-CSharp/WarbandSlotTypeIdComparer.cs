using System.Collections.Generic;

public class WarbandSlotTypeIdComparer : IEqualityComparer<WarbandSlotTypeId>
{
	public static readonly WarbandSlotTypeIdComparer Instance = new WarbandSlotTypeIdComparer();

	public bool Equals(WarbandSlotTypeId x, WarbandSlotTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(WarbandSlotTypeId obj)
	{
		return (int)obj;
	}
}
