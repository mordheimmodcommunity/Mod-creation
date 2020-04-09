using System.Collections.Generic;

public class EdgeSorter : IComparer<MapEdge>
{
	int IComparer<MapEdge>.Compare(MapEdge x, MapEdge y)
	{
		if (x.idx < y.idx)
		{
			return 1;
		}
		if (x.idx > y.idx)
		{
			return -1;
		}
		return 0;
	}
}
