using System.Collections.Generic;

public class UnitBaseIdComparer : IEqualityComparer<UnitBaseId>
{
	public static readonly UnitBaseIdComparer Instance = new UnitBaseIdComparer();

	public bool Equals(UnitBaseId x, UnitBaseId y)
	{
		return x == y;
	}

	public int GetHashCode(UnitBaseId obj)
	{
		return (int)obj;
	}
}
