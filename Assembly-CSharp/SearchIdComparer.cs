using System.Collections.Generic;

public class SearchIdComparer : IEqualityComparer<SearchId>
{
    public static readonly SearchIdComparer Instance = new SearchIdComparer();

    public bool Equals(SearchId x, SearchId y)
    {
        return x == y;
    }

    public int GetHashCode(SearchId obj)
    {
        return (int)obj;
    }
}
