using System.Collections.Generic;

public class PrimaryObjectiveResultIdComparer : IEqualityComparer<PrimaryObjectiveResultId>
{
	public static readonly PrimaryObjectiveResultIdComparer Instance = new PrimaryObjectiveResultIdComparer();

	public bool Equals(PrimaryObjectiveResultId x, PrimaryObjectiveResultId y)
	{
		return x == y;
	}

	public int GetHashCode(PrimaryObjectiveResultId obj)
	{
		return (int)obj;
	}
}
