using System.Collections.Generic;

public class TargetingIdComparer : IEqualityComparer<TargetingId>
{
	public static readonly TargetingIdComparer Instance = new TargetingIdComparer();

	public bool Equals(TargetingId x, TargetingId y)
	{
		return x == y;
	}

	public int GetHashCode(TargetingId obj)
	{
		return (int)obj;
	}
}
