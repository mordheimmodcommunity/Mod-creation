using System.Collections.Generic;

public class CriticalEffectIdComparer : IEqualityComparer<CriticalEffectId>
{
	public static readonly CriticalEffectIdComparer Instance = new CriticalEffectIdComparer();

	public bool Equals(CriticalEffectId x, CriticalEffectId y)
	{
		return x == y;
	}

	public int GetHashCode(CriticalEffectId obj)
	{
		return (int)obj;
	}
}
