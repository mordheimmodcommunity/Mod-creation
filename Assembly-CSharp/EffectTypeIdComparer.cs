using System.Collections.Generic;

public class EffectTypeIdComparer : IEqualityComparer<EffectTypeId>
{
	public static readonly EffectTypeIdComparer Instance = new EffectTypeIdComparer();

	public bool Equals(EffectTypeId x, EffectTypeId y)
	{
		return x == y;
	}

	public int GetHashCode(EffectTypeId obj)
	{
		return (int)obj;
	}
}
