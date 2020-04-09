using System.Collections.Generic;

public class UnitActionIdComparer : IEqualityComparer<UnitActionId>
{
	public static readonly UnitActionIdComparer Instance = new UnitActionIdComparer();

	public bool Equals(UnitActionId x, UnitActionId y)
	{
		return x == y;
	}

	public int GetHashCode(UnitActionId obj)
	{
		return (int)obj;
	}
}
