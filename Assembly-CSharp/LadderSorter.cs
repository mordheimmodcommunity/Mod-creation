using System.Collections.Generic;

public class LadderSorter : IComparer<UnitController>
{
	int IComparer<UnitController>.Compare(UnitController x, UnitController y)
	{
		if (x.unit.Initiative < y.unit.Initiative)
		{
			return 1;
		}
		if (x.unit.Initiative > y.unit.Initiative)
		{
			return -1;
		}
		return 0;
	}
}
