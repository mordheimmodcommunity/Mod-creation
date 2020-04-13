using System.Collections.Generic;

public class UnitSlotTypeIdComparer : IEqualityComparer<UnitSlotTypeId>
{
    public static readonly UnitSlotTypeIdComparer Instance = new UnitSlotTypeIdComparer();

    public bool Equals(UnitSlotTypeId x, UnitSlotTypeId y)
    {
        return x == y;
    }

    public int GetHashCode(UnitSlotTypeId obj)
    {
        return (int)obj;
    }
}
