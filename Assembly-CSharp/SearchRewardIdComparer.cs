using System.Collections.Generic;

public class SearchRewardIdComparer : IEqualityComparer<SearchRewardId>
{
	public static readonly SearchRewardIdComparer Instance = new SearchRewardIdComparer();

	public bool Equals(SearchRewardId x, SearchRewardId y)
	{
		return x == y;
	}

	public int GetHashCode(SearchRewardId obj)
	{
		return (int)obj;
	}
}
