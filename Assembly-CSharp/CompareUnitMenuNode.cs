using System.Collections.Generic;

public class CompareUnitMenuNode : IComparer<MenuNode>
{
    int IComparer<MenuNode>.Compare(MenuNode x, MenuNode y)
    {
        return x.name.CompareTo(y.name);
    }
}
