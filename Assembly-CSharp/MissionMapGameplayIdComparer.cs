using System.Collections.Generic;

public class MissionMapGameplayIdComparer : IEqualityComparer<MissionMapGameplayId>
{
	public static readonly MissionMapGameplayIdComparer Instance = new MissionMapGameplayIdComparer();

	public bool Equals(MissionMapGameplayId x, MissionMapGameplayId y)
	{
		return x == y;
	}

	public int GetHashCode(MissionMapGameplayId obj)
	{
		return (int)obj;
	}
}
