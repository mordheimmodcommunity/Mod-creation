using System.Collections.Generic;

public class PlayerRankIdComparer : IEqualityComparer<PlayerRankId>
{
	public static readonly PlayerRankIdComparer Instance = new PlayerRankIdComparer();

	public bool Equals(PlayerRankId x, PlayerRankId y)
	{
		return x == y;
	}

	public int GetHashCode(PlayerRankId obj)
	{
		return (int)obj;
	}
}
