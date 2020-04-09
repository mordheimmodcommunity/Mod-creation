using System.Collections.Generic;

public class AiUnitIdComparer : IEqualityComparer<AiUnitId>
{
	public static readonly AiUnitIdComparer Instance = new AiUnitIdComparer();

	public bool Equals(AiUnitId x, AiUnitId y)
	{
		return x == y;
	}

	public int GetHashCode(AiUnitId obj)
	{
		return (int)obj;
	}
}
