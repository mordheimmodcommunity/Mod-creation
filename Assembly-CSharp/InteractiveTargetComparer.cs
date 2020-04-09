using System.Collections.Generic;

public class InteractiveTargetComparer : IComparer<InteractiveTarget>
{
	int IComparer<InteractiveTarget>.Compare(InteractiveTarget x, InteractiveTarget y)
	{
		if (x.action.actionData.SortWeight < y.action.actionData.SortWeight)
		{
			return 1;
		}
		if (x.action.actionData.SortWeight > y.action.actionData.SortWeight)
		{
			return -1;
		}
		return 0;
	}
}
