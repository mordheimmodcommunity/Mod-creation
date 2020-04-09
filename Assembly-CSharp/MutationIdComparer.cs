using System.Collections.Generic;

public class MutationIdComparer : IEqualityComparer<MutationId>
{
	public static readonly MutationIdComparer Instance = new MutationIdComparer();

	public bool Equals(MutationId x, MutationId y)
	{
		return x == y;
	}

	public int GetHashCode(MutationId obj)
	{
		return (int)obj;
	}
}
