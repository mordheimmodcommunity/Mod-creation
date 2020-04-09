using System.Collections.Generic;

public class SearchZoneIdComparer : IEqualityComparer<SearchZoneId>
{
	public static readonly SearchZoneIdComparer Instance = new SearchZoneIdComparer();

	public bool Equals(SearchZoneId x, SearchZoneId y)
	{
		return x == y;
	}

	public int GetHashCode(SearchZoneId obj)
	{
		return (int)obj;
	}
}
