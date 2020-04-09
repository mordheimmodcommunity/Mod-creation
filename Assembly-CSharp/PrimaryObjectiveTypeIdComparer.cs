using System.Collections.Generic;

public class PrimaryObjectiveTypeIdComparer : IEqualityComparer<PrimaryObjectiveTypeId>
{
	public static readonly PrimaryObjectiveTypeIdComparer Instance = new PrimaryObjectiveTypeIdComparer();

	public bool Equals(PrimaryObjectiveTypeId x, PrimaryObjectiveTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(PrimaryObjectiveTypeId obj)
	{
		return (int)obj;
	}
}
