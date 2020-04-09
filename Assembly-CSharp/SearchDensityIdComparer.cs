using System.Collections.Generic;

public class SearchDensityIdComparer : IEqualityComparer<SearchDensityId>
{
	public static readonly SearchDensityIdComparer Instance = new SearchDensityIdComparer();

	public bool Equals(SearchDensityId x, SearchDensityId y)
	{
		return x == y;
	}

	public int GetHashCode(SearchDensityId obj)
	{
		return (int)obj;
	}
}
