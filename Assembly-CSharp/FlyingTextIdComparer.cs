using System.Collections.Generic;

public class FlyingTextIdComparer : IEqualityComparer<FlyingTextId>
{
	public static readonly FlyingTextIdComparer Instance = new FlyingTextIdComparer();

	public bool Equals(FlyingTextId x, FlyingTextId y)
	{
		return x == y;
	}

	public int GetHashCode(FlyingTextId obj)
	{
		return (int)obj;
	}
}
