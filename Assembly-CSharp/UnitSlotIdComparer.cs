using System.Collections.Generic;

public class UnitSlotIdComparer : IEqualityComparer<UnitSlotId>
{
	public static readonly UnitSlotIdComparer Instance = new UnitSlotIdComparer();

	public bool Equals(UnitSlotId x, UnitSlotId y)
	{
		return x == y;
	}

	public int GetHashCode(UnitSlotId obj)
	{
		return (int)obj;
	}
}
