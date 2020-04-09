using System.Collections.Generic;

public class DistrictIdComparer : IEqualityComparer<DistrictId>
{
	public static readonly DistrictIdComparer Instance = new DistrictIdComparer();

	public bool Equals(DistrictId x, DistrictId y)
	{
		return x == y;
	}

	public int GetHashCode(DistrictId obj)
	{
		return (int)obj;
	}
}
