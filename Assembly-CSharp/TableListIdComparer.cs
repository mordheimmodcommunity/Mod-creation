using System.Collections.Generic;

public class TableListIdComparer : IEqualityComparer<TableListId>
{
	public static readonly TableListIdComparer Instance = new TableListIdComparer();

	public bool Equals(TableListId x, TableListId y)
	{
		return x == y;
	}

	public int GetHashCode(TableListId obj)
	{
		return (int)obj;
	}
}
