using System.Collections.Generic;

public class SkirmishMapSorter : IComparer<SkirmishMap>
{
	int IComparer<SkirmishMap>.Compare(SkirmishMap x, SkirmishMap y)
	{
		if (x.mapData.Idx > y.mapData.Idx)
		{
			return 1;
		}
		if (x.mapData.Idx < y.mapData.Idx)
		{
			return -1;
		}
		return 0;
	}
}
