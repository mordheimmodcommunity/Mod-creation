using System.Collections.Generic;

public class PrimaryObjectiveIdComparer : IEqualityComparer<PrimaryObjectiveId>
{
	public static readonly PrimaryObjectiveIdComparer Instance = new PrimaryObjectiveIdComparer();

	public bool Equals(PrimaryObjectiveId x, PrimaryObjectiveId y)
	{
		return x == y;
	}

	public int GetHashCode(PrimaryObjectiveId obj)
	{
		return (int)obj;
	}
}
