using System.Collections.Generic;

public class BuildingTypeIdComparer : IEqualityComparer<BuildingTypeId>
{
	public static readonly BuildingTypeIdComparer Instance = new BuildingTypeIdComparer();

	public bool Equals(BuildingTypeId x, BuildingTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(BuildingTypeId obj)
	{
		return (int)obj;
	}
}
