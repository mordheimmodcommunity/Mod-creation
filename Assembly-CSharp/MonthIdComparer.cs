using System.Collections.Generic;

public class MonthIdComparer : IEqualityComparer<MonthId>
{
    public static readonly MonthIdComparer Instance = new MonthIdComparer();

    public bool Equals(MonthId x, MonthId y)
    {
        return x == y;
    }

    public int GetHashCode(MonthId obj)
    {
        return (int)obj;
    }
}
