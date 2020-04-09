using System.Collections.Generic;

public class AchievementTargetIdComparer : IEqualityComparer<AchievementTargetId>
{
	public static readonly AchievementTargetIdComparer Instance = new AchievementTargetIdComparer();

	public bool Equals(AchievementTargetId x, AchievementTargetId y)
	{
		return x == y;
	}

	public int GetHashCode(AchievementTargetId obj)
	{
		return (int)obj;
	}
}
