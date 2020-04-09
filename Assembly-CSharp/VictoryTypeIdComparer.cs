using System.Collections.Generic;

public class VictoryTypeIdComparer : IEqualityComparer<VictoryTypeId>
{
	public static readonly VictoryTypeIdComparer Instance = new VictoryTypeIdComparer();

	public bool Equals(VictoryTypeId x, VictoryTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(VictoryTypeId obj)
	{
		return (int)obj;
	}
}
