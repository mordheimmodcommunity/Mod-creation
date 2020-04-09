using System.Collections.Generic;

public class BoneIdComparer : IEqualityComparer<BoneId>
{
	public static readonly BoneIdComparer Instance = new BoneIdComparer();

	public bool Equals(BoneId x, BoneId y)
	{
		return x == y;
	}

	public int GetHashCode(BoneId obj)
	{
		return (int)obj;
	}
}
