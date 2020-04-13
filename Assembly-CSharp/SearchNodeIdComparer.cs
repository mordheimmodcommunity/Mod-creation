using System.Collections.Generic;

public class SearchNodeIdComparer : IEqualityComparer<SearchNodeId>
{
    public static readonly SearchNodeIdComparer Instance = new SearchNodeIdComparer();

    public bool Equals(SearchNodeId x, SearchNodeId y)
    {
        return x == y;
    }

    public int GetHashCode(SearchNodeId obj)
    {
        return (int)obj;
    }
}
