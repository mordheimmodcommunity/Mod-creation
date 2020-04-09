using System.Collections.Generic;

public class HitRollSorter : IComparer<Tuple<int, UnitController>>
{
	int IComparer<Tuple<int, UnitController>>.Compare(Tuple<int, UnitController> x, Tuple<int, UnitController> y)
	{
		if (x.Item1 < y.Item1)
		{
			return -1;
		}
		if (x.Item1 > y.Item1)
		{
			return 1;
		}
		return 0;
	}
}
