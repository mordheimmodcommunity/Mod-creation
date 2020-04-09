using System.Collections.Generic;

public class AiFilterCheckIdComparer : IEqualityComparer<AiFilterCheckId>
{
	public static readonly AiFilterCheckIdComparer Instance = new AiFilterCheckIdComparer();

	public bool Equals(AiFilterCheckId x, AiFilterCheckId y)
	{
		return x == y;
	}

	public int GetHashCode(AiFilterCheckId obj)
	{
		return (int)obj;
	}
}
