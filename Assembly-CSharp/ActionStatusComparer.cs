using System;
using System.Collections.Generic;

public class ActionStatusComparer : IComparer<ActionStatus>
{
    int IComparer<ActionStatus>.Compare(ActionStatus x, ActionStatus y)
    {
        if (x.actionData.SortWeight < y.actionData.SortWeight)
        {
            return 1;
        }
        if (x.actionData.SortWeight > y.actionData.SortWeight)
        {
            return -1;
        }
        return string.Compare(x.actionData.Name, y.actionData.Name, StringComparison.OrdinalIgnoreCase);
    }
}
