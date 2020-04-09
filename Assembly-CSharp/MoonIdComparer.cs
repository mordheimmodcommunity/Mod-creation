using System.Collections.Generic;

public class MoonIdComparer : IEqualityComparer<MoonId>
{
	public static readonly MoonIdComparer Instance = new MoonIdComparer();

	public bool Equals(MoonId x, MoonId y)
	{
		return x == y;
	}

	public int GetHashCode(MoonId obj)
	{
		return (int)obj;
	}
}
