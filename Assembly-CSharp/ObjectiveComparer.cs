using System.Collections.Generic;

public class ObjectiveComparer : IComparer<Objective>
{
	int IComparer<Objective>.Compare(Objective x, Objective y)
	{
		if (x.SortWeight < y.SortWeight)
		{
			return 1;
		}
		if (x.SortWeight > y.SortWeight)
		{
			return -1;
		}
		return 0;
	}
}
