using System.Collections.Generic;

public class UnitActionRefreshIdComparer : IEqualityComparer<UnitActionRefreshId>
{
	public static readonly UnitActionRefreshIdComparer Instance = new UnitActionRefreshIdComparer();

	public bool Equals(UnitActionRefreshId x, UnitActionRefreshId y)
	{
		return x == y;
	}

	public int GetHashCode(UnitActionRefreshId obj)
	{
		return (int)obj;
	}
}
