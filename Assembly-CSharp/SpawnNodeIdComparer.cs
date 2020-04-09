using System.Collections.Generic;

public class SpawnNodeIdComparer : IEqualityComparer<SpawnNodeId>
{
	public static readonly SpawnNodeIdComparer Instance = new SpawnNodeIdComparer();

	public bool Equals(SpawnNodeId x, SpawnNodeId y)
	{
		return x == y;
	}

	public int GetHashCode(SpawnNodeId obj)
	{
		return (int)obj;
	}
}
