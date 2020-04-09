using System.Collections.Generic;

public class WeekDayIdComparer : IEqualityComparer<WeekDayId>
{
	public static readonly WeekDayIdComparer Instance = new WeekDayIdComparer();

	public bool Equals(WeekDayId x, WeekDayId y)
	{
		return x == y;
	}

	public int GetHashCode(WeekDayId obj)
	{
		return (int)obj;
	}
}
