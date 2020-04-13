using System.Collections.Generic;

public class HolidayIdComparer : IEqualityComparer<HolidayId>
{
    public static readonly HolidayIdComparer Instance = new HolidayIdComparer();

    public bool Equals(HolidayId x, HolidayId y)
    {
        return x == y;
    }

    public int GetHashCode(HolidayId obj)
    {
        return (int)obj;
    }
}
