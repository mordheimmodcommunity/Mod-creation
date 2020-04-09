using System.Collections.Generic;

public class BuildingIdComparer : IEqualityComparer<BuildingId>
{
	public static readonly BuildingIdComparer Instance = new BuildingIdComparer();

	public bool Equals(BuildingId x, BuildingId y)
	{
		return x == y;
	}

	public int GetHashCode(BuildingId obj)
	{
		return (int)obj;
	}
}
