using System.Collections.Generic;

public class CompareUnitMenuController : IComparer<UnitMenuController>
{
    int IComparer<UnitMenuController>.Compare(UnitMenuController x, UnitMenuController y)
    {
        return WarbandMenuController.Compare(x, y);
    }
}
