using System.Collections.Generic;

public class AttributeTypeIdComparer : IEqualityComparer<AttributeTypeId>
{
	public static readonly AttributeTypeIdComparer Instance = new AttributeTypeIdComparer();

	public bool Equals(AttributeTypeId x, AttributeTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(AttributeTypeId obj)
	{
		return (int)obj;
	}
}
