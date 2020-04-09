using System.Collections.Generic;

public class SequenceIdComparer : IEqualityComparer<SequenceId>
{
	public static readonly SequenceIdComparer Instance = new SequenceIdComparer();

	public bool Equals(SequenceId x, SequenceId y)
	{
		return x == y;
	}

	public int GetHashCode(SequenceId obj)
	{
		return (int)obj;
	}
}
