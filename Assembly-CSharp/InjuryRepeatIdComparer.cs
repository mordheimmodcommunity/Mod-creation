using System.Collections.Generic;

public class InjuryRepeatIdComparer : IEqualityComparer<InjuryRepeatId>
{
	public static readonly InjuryRepeatIdComparer Instance = new InjuryRepeatIdComparer();

	public bool Equals(InjuryRepeatId x, InjuryRepeatId y)
	{
		return x == y;
	}

	public int GetHashCode(InjuryRepeatId obj)
	{
		return (int)obj;
	}
}
