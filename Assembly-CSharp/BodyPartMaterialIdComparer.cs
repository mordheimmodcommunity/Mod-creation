using System.Collections.Generic;

public class BodyPartMaterialIdComparer : IEqualityComparer<BodyPartMaterialId>
{
	public static readonly BodyPartMaterialIdComparer Instance = new BodyPartMaterialIdComparer();

	public bool Equals(BodyPartMaterialId x, BodyPartMaterialId y)
	{
		return x == y;
	}

	public int GetHashCode(BodyPartMaterialId obj)
	{
		return (int)obj;
	}
}
