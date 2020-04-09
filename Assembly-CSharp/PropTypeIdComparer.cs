using System.Collections.Generic;

public class PropTypeIdComparer : IEqualityComparer<PropTypeId>
{
	public static readonly PropTypeIdComparer Instance = new PropTypeIdComparer();

	public bool Equals(PropTypeId x, PropTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(PropTypeId obj)
	{
		return (int)obj;
	}
}
