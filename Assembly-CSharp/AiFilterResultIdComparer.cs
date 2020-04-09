using System.Collections.Generic;

public class AiFilterResultIdComparer : IEqualityComparer<AiFilterResultId>
{
	public static readonly AiFilterResultIdComparer Instance = new AiFilterResultIdComparer();

	public bool Equals(AiFilterResultId x, AiFilterResultId y)
	{
		return x == y;
	}

	public int GetHashCode(AiFilterResultId obj)
	{
		return (int)obj;
	}
}
