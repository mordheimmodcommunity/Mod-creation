using System.Collections.Generic;

public class AttackResultIdComparer : IEqualityComparer<AttackResultId>
{
	public static readonly AttackResultIdComparer Instance = new AttackResultIdComparer();

	public bool Equals(AttackResultId x, AttackResultId y)
	{
		return x == y;
	}

	public int GetHashCode(AttackResultId obj)
	{
		return (int)obj;
	}
}
