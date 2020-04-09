using System.Collections.Generic;

public class MarketEventIdComparer : IEqualityComparer<MarketEventId>
{
	public static readonly MarketEventIdComparer Instance = new MarketEventIdComparer();

	public bool Equals(MarketEventId x, MarketEventId y)
	{
		return x == y;
	}

	public int GetHashCode(MarketEventId obj)
	{
		return (int)obj;
	}
}
