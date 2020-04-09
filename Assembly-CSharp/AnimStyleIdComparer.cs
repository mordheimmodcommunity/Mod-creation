using System.Collections.Generic;

public class AnimStyleIdComparer : IEqualityComparer<AnimStyleId>
{
	public static readonly AnimStyleIdComparer Instance = new AnimStyleIdComparer();

	public bool Equals(AnimStyleId x, AnimStyleId y)
	{
		return x == y;
	}

	public int GetHashCode(AnimStyleId obj)
	{
		return (int)obj;
	}
}
