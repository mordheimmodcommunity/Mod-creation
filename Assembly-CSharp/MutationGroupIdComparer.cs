using System.Collections.Generic;

public class MutationGroupIdComparer : IEqualityComparer<MutationGroupId>
{
	public static readonly MutationGroupIdComparer Instance = new MutationGroupIdComparer();

	public bool Equals(MutationGroupId x, MutationGroupId y)
	{
		return x == y;
	}

	public int GetHashCode(MutationGroupId obj)
	{
		return (int)obj;
	}
}
