using System.Collections.Generic;

public class UnitActiveStatusIdComparer : IEqualityComparer<UnitActiveStatusId>
{
	public static readonly UnitActiveStatusIdComparer Instance = new UnitActiveStatusIdComparer();

	public bool Equals(UnitActiveStatusId x, UnitActiveStatusId y)
	{
		return x == y;
	}

	public int GetHashCode(UnitActiveStatusId obj)
	{
		return (int)obj;
	}
}
