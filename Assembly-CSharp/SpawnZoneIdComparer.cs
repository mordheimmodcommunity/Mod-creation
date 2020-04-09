using System.Collections.Generic;

public class SpawnZoneIdComparer : IEqualityComparer<SpawnZoneId>
{
	public static readonly SpawnZoneIdComparer Instance = new SpawnZoneIdComparer();

	public bool Equals(SpawnZoneId x, SpawnZoneId y)
	{
		return x == y;
	}

	public int GetHashCode(SpawnZoneId obj)
	{
		return (int)obj;
	}
}
