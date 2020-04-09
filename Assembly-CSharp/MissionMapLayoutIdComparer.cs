using System.Collections.Generic;

public class MissionMapLayoutIdComparer : IEqualityComparer<MissionMapLayoutId>
{
	public static readonly MissionMapLayoutIdComparer Instance = new MissionMapLayoutIdComparer();

	public bool Equals(MissionMapLayoutId x, MissionMapLayoutId y)
	{
		return x == y;
	}

	public int GetHashCode(MissionMapLayoutId obj)
	{
		return (int)obj;
	}
}
