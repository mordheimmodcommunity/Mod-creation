using System.Collections.Generic;

public class TableFilterIdComparer : IEqualityComparer<TableFilterId>
{
    public static readonly TableFilterIdComparer Instance = new TableFilterIdComparer();

    public bool Equals(TableFilterId x, TableFilterId y)
    {
        return x == y;
    }

    public int GetHashCode(TableFilterId obj)
    {
        return (int)obj;
    }
}
