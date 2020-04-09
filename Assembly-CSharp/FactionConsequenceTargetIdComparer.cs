using System.Collections.Generic;

public class FactionConsequenceTargetIdComparer : IEqualityComparer<FactionConsequenceTargetId>
{
	public static readonly FactionConsequenceTargetIdComparer Instance = new FactionConsequenceTargetIdComparer();

	public bool Equals(FactionConsequenceTargetId x, FactionConsequenceTargetId y)
	{
		return x == y;
	}

	public int GetHashCode(FactionConsequenceTargetId obj)
	{
		return (int)obj;
	}
}
